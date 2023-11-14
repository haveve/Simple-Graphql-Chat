import { webSocket } from 'rxjs/webSocket'
import { Chat, ChatParticipant, Message, DerivedMessageOrChatInfo, UserNotification, User } from '../Features/Types';
import { backDomain } from '../Features/Constants';
import { WebSocketSubject } from 'rxjs/webSocket'
import { Subject, NextObserver, interval, Subscription, Observable, Subscriber } from 'rxjs'
import { GetTokenObservable, StoredTokenType } from './AuthorizationRequests';
import { getCookie } from '../Features/Functions';
import { subscribe } from 'diagnostics_channel';

const keepAliveInterval = 60;

export type AllFieldsRequestType = {
    //query
    messages?: Message[],
    chats?: Chat[],
    participants?: ChatParticipant[],
    user?: User,
    //subscription
    chatNotification?: DerivedMessageOrChatInfo,
    userNotification?: UserNotification,
    //mutation
    addMessage?: Message,
    removeMessage?: Message,
    updateMessage?: Message,
    createChat?: Chat,
    removeChat?: number,
    updateChat?: Chat,
    addUserToChat?: string,
    removeUserFromChat?: string
}

export type subscriptionDataType<T, K> = {
    data: T
    error?: K[]
}

export type defaultSubscriptionResponse<T> = {
    id?: string,
    type: string,
    payload: T
}

export type MinWebSocketType = {
    payload: {
        authorization?: string
    }
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

    public constructor(webSocket: WebSocketSubject<T>, keepAliveMessage: T) {
        this.webSocket = webSocket
        this.keepAlive = interval(keepAliveInterval * 1000).subscribe({
            next: () => {
                if (!this.webSocket.closed) {
                    this.webSocket.next(keepAliveMessage)
                    return;
                }
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

    public next(next: T) {
        GetTokenObservable().subscribe({
            next: () => {
                const token: StoredTokenType = JSON.parse(getCookie("access_token")!)
                next.payload.authorization = token.token;
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

export function ConnectToChat(reconnect: boolean = false): Observable<WebSocketProxy<defaultSubscriptionResponse<any>>> {

    return new Observable(subscribe => {
        if ((!SingletonContainer.GetInstance() || reconnect || socket.closed) && SingletonContainer.getDone()) {
            SingletonContainer.setDone(false);
            GetTokenObservable().subscribe({
                next: () => {

                    const token: StoredTokenType = JSON.parse(getCookie("access_token")!)

                    socket = webSocket<defaultSubscriptionResponse<any>>({
                        url: `wss://${backDomain}/graphql?Authorization=${token.token}`,
                        protocol: 'graphql-ws'
                    })
                    SingletonContainer.SetInstance(new WebSocketProxy(socket, { type: "connection_keep_alive", payload: {} }));
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

