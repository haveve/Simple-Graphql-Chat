import { webSocket } from 'rxjs/webSocket'
import { Chat, ChatParticipant, Message, DerivedMessageOrChatInfo, DrivedUserOrChatInfo, User, FullChat } from '../Features/Types';
import { backDomain } from '../Features/Constants';
import { WebSocketSubject } from 'rxjs/webSocket'
import { NextObserver, interval, Subscription, Observable, Subscriber, mergeMap, map, catchError, of } from 'rxjs'
import { GetTokenObservable, StoredTokenType } from './AuthorizationRequests';
import { getCookie } from '../Features/Functions';
import { nanoid } from 'nanoid';
import store from '../Redux/store';
import { PayloadAction } from '@reduxjs/toolkit';
import { UpdateUserResult } from '../Redux/Slicers/UserSlicer';
import { ajax } from 'rxjs/ajax';
import { response } from './AuthorizationRequests';
import Dispatch from '../SocketDispatcher';
import i18next from 'i18next';

const dispatch = store.dispatch

export const MaxFileSizeInKB = 150;

const keepAliveInterval = 60;

export type AllFieldsRequestType = {
    //query
    messages?: Message[],
    chats?: Chat[],
    participants?: ChatParticipant[],
    user?: User,
    chatFullInfo?: FullChat
    //subscription
    chatNotification?: DerivedMessageOrChatInfo,
    userNotification?: DrivedUserOrChatInfo,
    //mutation
    addMessage?: Message,
    removeMessage?: Message,
    updateMessage?: Message,
    createChat?: FullChat,
    removeChat?: number,
    updateChat?: FullChat,
    addUserToChat?: string,
    removeUserFromChat?: string,
    updateUser?: UpdateUserResult,
    deleteUser?: number
}

export type subscriptionDataType<T, K> = {
    data: T
    errors?: K[]
}

export type defaultSubscriptionResponse<T> = {
    id?: string,
    type: string,
    payload: T
}

export type MinWebSocketType = {
    payload: {
        authorization?: string
        variables?: { authorization: string }
    }
    type: string
}

class SingletonContainer {
    private static instance: any
    private static done: boolean = true

    public static SetInstance(data: any) {
        this.instance = data
    }

    public static GetInstance() {
        return this.instance
    }

    public static setDone(done: boolean) {
        this.done = done
    }

    public static getDone() {
        return this.done
    }
}

class WebSocketProxy<T extends MinWebSocketType>{

    private webSocket: WebSocketSubject<T>
    private keepAlive: Subscription
    private skipPinging: boolean

    public constructor(webSocketFanctory: () => [WebSocketSubject<T>, T], keepAliveMessage: T, Dispatch: (data: any) => void) {

        const [socket, init] = webSocketFanctory();
        this.skipPinging = false;
        this.webSocket = socket
        this.webSocket.subscribe({
            next: (response) => {
                Dispatch(response)
            },
            error: (error) => {
                console.log(JSON.stringify(error))
            }
        })
        this.keepAlive = interval(keepAliveInterval * 1000).subscribe({
            next: () => {
                if (!this.webSocket.closed && !this.skipPinging) {
                    this.next(keepAliveMessage, null)
                }
                this.skipPinging = false;
            }
        })
        this.next(init, null)
    }

    public subscribe(subscriber: NextObserver<any>) {
        return this.webSocket.subscribe(subscriber)
    }

    public complete() {
        this.webSocket.complete()
        this.keepAlive.unsubscribe()
    }

    public next(next: T, paddingAction: PayloadAction<any> | null) {
        if (paddingAction) {
            dispatch(paddingAction)
            this.skipPinging = true;
        }
        GetTokenObservable().subscribe({
            next: () => {
                const token: StoredTokenType = JSON.parse(getCookie("access_token")!)
                if (next.type === 'connection_init')
                    next.payload.authorization = token.token;
                else {
                    if (!next.payload.variables)
                        next.payload.variables = { authorization: token.token }
                    next.payload.variables.authorization = token.token
                }
                this.webSocket.next(next)
            }
        })
    }

}

function SendProxy(subscriber: Subscriber<WebSocketProxy<defaultSubscriptionResponse<any>>>) {
    const inter = interval(10).subscribe(() => {
        if (SingletonContainer.getDone()) {
            inter.unsubscribe()
            subscriber.next(SingletonContainer.GetInstance())
        }
    })
}

export function GetNewToken() {
    return GetTokenObservable(true);
}

export function GetWebSocket(): [WebSocketSubject<defaultSubscriptionResponse<any>>, defaultSubscriptionResponse<any>] {
    return [webSocket<defaultSubscriptionResponse<any>>({
        url: `wss://${backDomain}/graphql`,
        protocol: 'graphql-ws'
    }), { "type": "connection_init", "payload": {} }]
}

export function ConnectToChat(reconnect: boolean = false, newToken: boolean = false): Observable<WebSocketProxy<defaultSubscriptionResponse<any>>> {

    return new Observable(subscribe => {
        if ((!SingletonContainer.GetInstance() || reconnect) && SingletonContainer.getDone()) {
            SingletonContainer.setDone(false);
            GetTokenObservable(newToken).subscribe({
                next: () => {

                    SingletonContainer.SetInstance(new WebSocketProxy(GetWebSocket, {
                        id: nanoid(), type: "start", payload: {
                            query: `query{
                        ping
                      }`}
                    }, Dispatch));
                    const proxy = SingletonContainer.GetInstance();
                    SingletonContainer.setDone(true);
                    subscribe.next(proxy)
                }
            })
        }
        else {
            SendProxy(subscribe)
        }
    })
}

export function ajaxUploadFile<T>(file: File, variableName: string, query: string, variables: { [key: string]: any } = {}) {

    if (file.size > MaxFileSizeInKB * 1024) {
        throw `${i18next.t('FileSizeError')} ${MaxFileSizeInKB} KB`
    }

    return GetTokenObservable().pipe(mergeMap(() => {
        const token: StoredTokenType = JSON.parse(getCookie("access_token")!)

        var formData = new FormData()
        variables[variableName] = null;
        variables["authorization"] = token.token;

        formData.append('operations', `{"query":"${query}","variables":${JSON.stringify(variables)},"operationName":null}`);
        formData.append('map', JSON.stringify({ "0": [`variables.${variableName}`] }));
        formData.append('0', file)

        return ajax<response<T>>({
            url: `https://${backDomain}/graphql`,
            method: "POST",
            headers: {
                Accept: '*/*',
                'Authorization': 'Bearer ' + token.token
            },
            body: formData
        }).pipe(map((response) => {
            const data = response.response;
            if (data.errors) {
                console.log(JSON.stringify(data.errors))
                throw 'error'
            }

            return data.data;
        }))
    }))
}
