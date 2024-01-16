import { baseUrl } from '../Features/Constants';
import { ajax } from 'rxjs/ajax';
import { map, catchError, Observable, timer, mergeMap } from 'rxjs';
import { LogoutDeleteCookie, setCookie, getCookie } from '../Features/Functions';
import { set2fMutation, get2fQuery, drop2fMutation, verify2fQuery } from '../Features/Queries';

export const url = `${baseUrl}/graphql-auth`

export type response<T = any, K = any> = {
    data: T,
    errors?: K
}

export enum RefreshStatus {
    DoRefresh,
    DonotRefresh,
    ThereIsNoRefreshes
}

export function TokenErrorHandler() {
    LogoutDeleteCookie()
    window.location.pathname = '/'
}

export function GetTokenObservable(forsed: boolean = false) {
    return DoRefresh(WhetherDoRefresh(forsed))
}

export type DoRefreshType = {
    refresh_token: string,
    refreshStatus: RefreshStatus
}



export function DoRefresh(refresh: DoRefreshType) {
    switch (refresh.refreshStatus) {
        case RefreshStatus.DoRefresh:
            const refreshSentString = getCookie("refresh_sent");
            const isTokenSent: boolean = refreshSentString ? JSON.parse(refreshSentString) : refreshSentString
            if (!isTokenSent) {
                setCookie({ name: "refresh_sent", value: "true" })
                ajaxForRefresh({}, refresh.refresh_token).subscribe({
                    error: () => {
                        TokenErrorHandler()
                    },
                    next: () => {
                        setCookie({ name: "refresh_sent", value: "false" })
                    }
                })
            }
            break;
        case RefreshStatus.DonotRefresh:
            break;
        case RefreshStatus.ThereIsNoRefreshes:
            TokenErrorHandler();
            break;
    }

    return new Observable<void>((subscriber) => {
        const sub = timer(10, 20).subscribe({
            next: () => {
                let refreshSentString = getCookie("refresh_sent");

                if (!refreshSentString) {
                    sub.unsubscribe();
                    return;
                }

                let isTokenSent: boolean = JSON.parse(refreshSentString)
                if (!isTokenSent) {
                    subscriber.next()
                    sub.unsubscribe()
                }
            }
        })
    })


}

export function GetRefresh() {
    const refreshTokenJson = getCookie("refresh_token");

    if (!refreshTokenJson) {
        return ""
    }
    const refreshTokenObj: StoredTokenType = JSON.parse(refreshTokenJson);
    if (!refreshTokenObj.token) {
        return ""
    }
    return refreshTokenObj.token
}

export function WhetherDoRefresh(forced: boolean = false): DoRefreshType {

    const refreshTokenJson = getCookie("refresh_token");
    const accessTokenJson = getCookie("access_token");

    if (refreshTokenJson) {
        const refreshTokenObj: StoredTokenType = JSON.parse(refreshTokenJson);
        if (accessTokenJson && !forced) {

            const accessTokenObj: StoredTokenType = JSON.parse(accessTokenJson)
            const nowInSeconds = new Date().getTime();
            if (!accessTokenObj
                || accessTokenObj.expiredAt - nowInSeconds < 2000) {
                return {
                    refresh_token: refreshTokenObj.token,
                    refreshStatus: RefreshStatus.DoRefresh
                }
            }
            return {
                refresh_token: "",
                refreshStatus: RefreshStatus.DonotRefresh
            }
        }
        return {
            refresh_token: refreshTokenObj.token,
            refreshStatus: RefreshStatus.DoRefresh
        }
    }
    return {
        refresh_token: "",
        refreshStatus: RefreshStatus.ThereIsNoRefreshes
    }

}

export enum TokenAjaxStatus {
    Ok,
    Error
}

export function GetAjaxObservable<T, K>(query: string, variables: {}, url: string, refreshToken: string | null = null, withCredentials = false, skipContentType: boolean = false) {

    const tokenString = getCookie("access_token");

    let tokenHeader: any = {}

    if (tokenString) {
        const token: StoredTokenType = JSON.parse(tokenString)
        tokenHeader = {
            'Authorization': 'Bearer ' + token.token,
        }
    }

    if (refreshToken) {
        tokenHeader = {
            refresh_token: refreshToken,
        }
    }

    if (!skipContentType) {
        tokenHeader['Content-Type'] = 'application/json'
    }


    return ajax<response<T, K>>({
        url,
        method: "POST",
        headers: {
            ...tokenHeader
        },
        body: {
            query,
            variables
        },
        withCredentials: withCredentials
    })
}


export function isUnvalidTokenError(response: {
    "errors": [
        {
            "message": string
        }
    ],
    "data": {
        "refreshToken": {
            "refresh_token": string,
            "user_id": number,
            "access_token": string
        }
    }
}) {
    const errors = response.errors;
    if (errors && response.data.refreshToken) {
        return true;
    }
    return false;
}

export type LoginErrorType = {
    message: string
}

export type RequestTokenType = {
    issuedAt: Date,
    token: string
    expiredAt: Date,
}

export type StoredTokenType = {
    issuedAt: number,
    token: string
    expiredAt: number,
}

export type LoginType = {
    login: {
        access_token: RequestTokenType,
        user_id: string,
        refresh_token: RequestTokenType,
        redirect_url: string | null,
    }
}

export type RefreshType = {
    refreshToken: {
        access_token: RequestTokenType,
        user_id: string,
        refresh_token: RequestTokenType
    }
}

export type RegistrationType = {
    registration: {
        nickName: string,
        email: string
    }
}

export type SetPasswordByCodeType = {
    code: string,
    password: string,
    email: string
}

export function ajaxSetPasswordByCode(variables: SetPasswordByCodeType) {
    return GetAjaxObservable<string, any>(`mutation($code:String!,$password:String!,$email:String!){
        resetUserPasswordByCode(code:$code,password:$password,email:$email)
      }`, variables, url).pipe(map(response => {
        let fullResponse = response.response;

        if (fullResponse.errors)
            throw fullResponse.errors[0].message;
    }))
}

export function RequestPasswordReset(user: String) {
    return GetAjaxObservable<string, any>(`mutation sentResetPasswordEmail($user: String!){
        sentResetPasswordEmail(nickNameOrEmail: $user)
  }`, { user }, url,).pipe(
        map(response => {
            let fullResponse = response.response;

            if (fullResponse.errors)
                throw fullResponse.errors[0].message;
        })
    );
}


export function ajaxForRegistration(variables: RegistrationType) {
    return GetAjaxObservable<string, any>(`mutation($registration:RegistrationInput!){
        registration(registration:$registration)
      }`, variables, url).pipe(map(response => {
        let fullResponse = response.response;

        if (fullResponse.errors)
            throw fullResponse.errors[0].message;
    }))
}

export function ajaxForLogin(variables: {}) {
    return GetAjaxObservable<LoginType, LoginErrorType[]>(`query($login:LoginInput!){
        login(login:$login){
          access_token {
            issuedAt
            token
            expiredAt
          }
          user_id
          refresh_token {
            issuedAt
            token
            expiredAt
          }
          redirect_url
        }
      }`, variables, url).pipe(
        map((value): string => {


            let fullResponse = value.response;
            let response = fullResponse.data.login;


            if (fullResponse.errors)
                throw fullResponse.errors[0].message;

            if (response.redirect_url) {
                return response.redirect_url;
            }


            const access_token_to_save: StoredTokenType = {
                issuedAt: new Date(response.access_token.issuedAt).getTime(),
                expiredAt: new Date(response.access_token.expiredAt).getTime(),
                token: response.access_token.token
            }

            const refresh_token_to_save: StoredTokenType = {
                issuedAt: new Date(response.refresh_token.issuedAt).getTime(),
                expiredAt: new Date(response.refresh_token.expiredAt).getTime(),
                token: response.refresh_token.token
            }

            setCookie({
                name: "access_token",
                value: JSON.stringify(access_token_to_save),
                expires_second: access_token_to_save.expiredAt / 1000,
                path: "/"
            });
            setCookie({
                name: "user_id",
                value: response.user_id,
                expires_second: access_token_to_save.expiredAt / 1000,
                path: "/"
            });
            setCookie({
                name: "refresh_token",
                value: JSON.stringify(refresh_token_to_save),
                expires_second: refresh_token_to_save.expiredAt / 1000,
                path: "/"
            });

            return "/main";
        }),
        catchError((error) => {
            throw error
        })
    );
}


export function IsRefreshError(error: any) {
    const refreshError: RefreshError = error
    return refreshError.error && refreshError.errorType === RefreshErrorEnum.RefreshError;
}


export enum RefreshErrorEnum {
    RefreshError
}

export type RefreshError = {
    error: string,
    errorType: RefreshErrorEnum
}

export function ajaxForRefresh(variables: {}, token: string) {
    return GetAjaxObservable<RefreshType, LoginErrorType[]>(`query{
        refreshToken{
          access_token {
            issuedAt
            token
            expiredAt
          }
          user_id
          refresh_token {
            issuedAt
            token
            expiredAt
          }
        }
        }`, variables, url, token).pipe(
        map((value): void => {
            let response = value.response.data.refreshToken;
            let refreshError: RefreshError = {
                error: "",
                errorType: RefreshErrorEnum.RefreshError
            }

            if (isUnvalidTokenError(value.response as any)) {
                LogoutDeleteCookie()
                refreshError.error = response.refresh_token.token
                throw refreshError;
            }

            if (value.response.errors)
                throw refreshError;

            const access_token_to_save: StoredTokenType = {
                issuedAt: new Date(response.access_token.issuedAt).getTime(),
                expiredAt: new Date(response.access_token.expiredAt).getTime(),
                token: response.access_token.token
            }

            const refresh_token_to_save: StoredTokenType = {
                issuedAt: new Date(response.refresh_token.issuedAt).getTime(),
                expiredAt: new Date(response.refresh_token.expiredAt).getTime(),
                token: response.refresh_token.token
            }
            setCookie({
                name: "access_token",
                value: JSON.stringify(access_token_to_save),
                expires_second: access_token_to_save.expiredAt / 1000,
                path: "/"
            });
            setCookie({
                name: "user_id",
                value: response.user_id,
                expires_second: access_token_to_save.expiredAt / 1000,
                path: "/"
            });
            setCookie({
                name: "refresh_token",
                value: JSON.stringify(refresh_token_to_save),
                expires_second: refresh_token_to_save.expiredAt / 1000,
                path: "/"
            });
        }),
        catchError((error) => {
            throw error
        })
    );
}

export function ajaxForLogout(token: string) {
    return ajax({
        url: url,
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
            refresh_token: token,
        },
        body: JSON.stringify({
            query: `query{
          logout
        }`,
        }),
        withCredentials: true,
    }).pipe(
        map((res: any): void => {

            if (res.response.errors) {
                console.error(JSON.stringify(res.response.errors))
                throw "error"
            }

            return res;
        }),
        catchError((error) => {
            throw error
        })
    );
}

export function ajaxForAuthorizedRequests<T, K>(query: string, variables: { [key: string]: any }, url: string = `${baseUrl}/graphql`) {
    return GetTokenObservable().pipe(mergeMap(() => {
        const tokenString = getCookie("access_token");

        if (!tokenString) {
            throw 'invalid access token'
        }

        const token: StoredTokenType = JSON.parse(tokenString)
        variables["authorized"] = token.token
        return GetAjaxObservable<T, K>(query, variables, url).pipe(map(response => {
            const res = response.response;
            if (res.errors) {
                console.log(JSON.stringify(res.errors));
                throw 'error'
            }
            return res.data;
        }))
    }));
}

export interface _2fAuthResult {
    qrUrl: string,
    manualEntry: string,
    key: string
}

export interface _2fAuthResultGraph {
    get2fAuth: _2fAuthResult
}


export function ajaxFor2fAuth() {
    return ajaxForAuthorizedRequests<_2fAuthResultGraph, any>(get2fQuery, {}, url).pipe(map(data => data.get2fAuth))
}

export interface _set2fResultGraph {
    set2fAuth: string
}

export function axajSetUser2fAuth(key: string, code: string) {
    return ajaxForAuthorizedRequests<_set2fResultGraph, any>(set2fMutation, { data: { key, code } }, url).pipe(map(response => {
        const data = response.set2fAuth;
        if (data) {
            const toBlob = `Reset token = ${data}`
            const blob = new Blob([toBlob], { type: 'text/plain' });
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = '_2f_code_reset.txt';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    }))
}

export interface verifyUserCodeGraph {
    verify2fAuth: string
}

export function ajaxVerifyUserCode(token: string, code: string, loginPath: string) {
    return GetAjaxObservable<verifyUserCodeGraph, any>(verify2fQuery, { token, code }, url).pipe(map((response) => {
        const data = response.response;
        if (data.errors) {
            throw 'error'
        }
        return loginPath + data.data.verify2fAuth
    }))
}

export interface _2fDroGraph {
    drop2fAuth: string
}

export function ajaxFor2fDrop(code: string) {
    return ajaxForAuthorizedRequests<_2fDroGraph, any>(drop2fMutation, { code }, url).pipe(map((response) => {
        return response.drop2fAuth
    }))
}