import { webSocket } from 'rxjs/webSocket'
import { Chat, ChatParticipant, Message, DerivedMessageOrChatInfo, UserNotification, User, FullChat } from '../Features/Types';
import { backDomain } from '../Features/Constants';
import { WebSocketSubject } from 'rxjs/webSocket'
import { NextObserver, interval, Subscription, Observable, Subscriber } from 'rxjs'
import { GetTokenObservable, StoredTokenType } from './AuthorizationRequests';
import { getCookie } from '../Features/Functions';
import { nanoid } from 'nanoid';
import store from '../Redux/store';
import { dropCurrentChat, setError } from '../Redux/Slicers/ChatSlicer';
import { setState } from '../Redux/Slicers/ChatSlicer';
import { defaultErrorMessage } from '../Features/Constants';
import { PayloadAction } from '@reduxjs/toolkit';
const dispatch = store.dispatch

const keepAliveInterval = 60;

export type AllFieldsRequestType = {
    //query
    messages?: Message[],
    chats?: Chat[],
    participants?: ChatParticipant[],
    user?: User,
    chatFullInfo?:FullChat
    //subscription
    chatNotification?: DerivedMessageOrChatInfo,
    userNotification?: UserNotification,
    //mutation
    addMessage?: Message,
    removeMessage?: Message,
    updateMessage?: Message,
    createChat?: FullChat,
    removeChat?: number,
    updateChat?: FullChat,
    addUserToChat?: string,
    removeUserFromChat?: string
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
    private skipPinging:boolean

    public constructor(webSocket: WebSocketSubject<T>, keepAliveMessage: T) {
        this.skipPinging = false;
        this.webSocket = webSocket
        this.webSocket.subscribe({
            error: (error) => {
                console.log(JSON.stringify(error))
                if (this.webSocket.closed) {
                    this.keepAlive.unsubscribe();
                    dispatch(dropCurrentChat())
                }
                dispatch(setError(defaultErrorMessage))
            }
        })
        this.keepAlive = interval(keepAliveInterval * 1000).subscribe({
            next: () => {
                if (!this.webSocket.closed && !this.skipPinging) {
                    this.next(keepAliveMessage,null)
                }
                this.skipPinging = false;
            }
        })
    }

    public subscribe(subscriber: NextObserver<any>) {
        this.webSocket.subscribe(subscriber)
    }

    public complete() {
        this.webSocket.complete()
        this.keepAlive.unsubscribe()
    }

    public next(next: T, paddingAction:PayloadAction<any>|null) {
        this.skipPinging = true;
        if(paddingAction)
             dispatch(paddingAction)
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


let socket: WebSocketSubject<defaultSubscriptionResponse<any>>;

export function GetNewToken(){
    return GetTokenObservable(true);
}

export function ConnectToChat(reconnect: boolean = false, newToken:boolean = false): Observable<WebSocketProxy<defaultSubscriptionResponse<any>>> {

    return new Observable(subscribe => {
        if ((!SingletonContainer.GetInstance() || reconnect || socket.closed) && SingletonContainer.getDone()) {
            SingletonContainer.setDone(false);
            GetTokenObservable(newToken).subscribe({
                next: () => {
                    socket = webSocket<defaultSubscriptionResponse<any>>({
                        url: `wss://${backDomain}/graphql`,
                        protocol: 'graphql-ws'
                    })
                    SingletonContainer.SetInstance(new WebSocketProxy(socket, {
                        id: nanoid(), type: "start", payload: {
                            query: `query{
                        ping
                      }`}
                    }));
                    const proxy = SingletonContainer.GetInstance();
                    proxy.next({ "type": "connection_init", "payload": {} })
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