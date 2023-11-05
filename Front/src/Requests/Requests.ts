import { webSocket } from 'rxjs/webSocket'
import { Message } from '../Features/Types';
import { backDomain } from '../Features/Constants';
import { WebSocketSubject } from 'rxjs/webSocket'
import { Subject, NextObserver } from 'rxjs'
import { GetTokenObservable, StoredTokenType } from './AuthorizationRequests';
import { getCookie } from '../Features/Functions';

export type AllFieldsRequestType = {
    messageAdded?: Message,
    messages?: Message[]
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

export class WebSocketProxy<T extends MinWebSocketType>{

    private webSocket: WebSocketSubject<T>

    public constructor(webSocket: WebSocketSubject<T>) {
        this.webSocket = webSocket
    }

    public subscribe(subscriber: NextObserver<any>) {
        this.webSocket.subscribe(subscriber)
    }

    public complete() {
        this.webSocket.complete()
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

export function ConnectToChat(): WebSocketProxy<defaultSubscriptionResponse<any>> {

    const socket = webSocket<defaultSubscriptionResponse<any>>({
        url: `wss://${backDomain}/graphql`,
        protocol: 'graphql-ws'
    })

    let proxy = new WebSocketProxy(socket);
    proxy.next({ "type": "connection_init", "payload": {} })

    return proxy;
}

