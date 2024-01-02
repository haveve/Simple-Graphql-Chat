import { backDomain } from '../Features/Constants';
import { ajax } from 'rxjs/ajax';
import { map, catchError, Observable, timer, mergeMap } from 'rxjs';
import { LogoutDeleteCookie, setCookie, getCookie } from '../Features/Functions';

export const url = `https://${backDomain}/graphql-auth`

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

const _2fAuthUrl = "https://" + backDomain + "/2f-auth"

export interface _2fAuthResult {
    qrUrl: string,
    manualEntry: string,
    key: string
}

export function ajaxFor2fAuth() {
    return GetTokenObservable().pipe(mergeMap(() => {
        const token: StoredTokenType = JSON.parse(getCookie("access_token")!)
        return ajax<_2fAuthResult>({
            url: _2fAuthUrl,
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token.token,
            }
        });
    }))
}

const _2fSetUrl = "https://" + backDomain + "/set-2f-auth";

export function axajSetUser2fAuth(key: string, code: string) {
    return GetTokenObservable().pipe(mergeMap(() => {
        const token: StoredTokenType = JSON.parse(getCookie("access_token")!)
        return ajax<string>({
            url: _2fSetUrl,
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token.token
            },
            body: JSON.stringify({
                key,
                code
            })
        }).pipe(map(response => {
            const data = response.response
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

    }))
}

const _2fVerifyServiceUrl = "https://" + backDomain + "/verify-2f-auth";

export function ajaxVerifyUserCode(token: string, code: string, loginPath: string) {
    return ajax<string>({
        url: _2fVerifyServiceUrl + `?token=${token}&code=${code}`
    }).pipe(map((response) => {
        return loginPath + response.response
    }))
}

const _2fDropUrl = "https://" + backDomain + "/drop-2f-auth"

export function ajaxFor2fDrop(code: string) {

    return GetTokenObservable().pipe(mergeMap(() => {
        const token: StoredTokenType = JSON.parse(getCookie("access_token")!)
        return ajax<string>({
            url: _2fDropUrl + `?code=${code}`,
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token.token,
            }
        })
    }))
}
