import { webSocket } from 'rxjs/webSocket'
import { Message } from '../Features/Types';
import { backDomain } from '../Features/Constants';
import { WebSocketSubject } from 'rxjs/webSocket'
import {Subject,Observer} from 'rxjs'


export type AllFieldsRequestType = {
    messageAdded?:Message,
    messages?:Message[]
}

export type subscriptionDataType<T,K> = {
    data:T
    error?:K[]
}

export type defaultSubscriptionResponse<T> = {
    id?:string,
    type:string,
    payload:T
}

export class WebSocketProxy<T>{

    private listOfQueue:defaultSubscriptionResponse<any>[]
    private webSocket:WebSocketSubject<T>

    public constructor(webSocket:WebSocketSubject<T>){
        this.webSocket = webSocket
        this.listOfQueue = []
    }

    public subscribe(subscriber:Observer<any>){
        this.webSocket.subscribe(subscriber)
    }

    public complete(){
        this.webSocket.complete()
    }

    public next(next:T){
        this.webSocket.next(next);
    }
    
}

export function ConnectToChat():WebSocketProxy<defaultSubscriptionResponse<any>>{

    const socket = webSocket<defaultSubscriptionResponse<any>>({
        url:`wss://${backDomain}/graphql`,
        protocol: 'graphql-ws'
    })

    let proxy = new WebSocketProxy(socket);
    proxy.next({ "type": "connection_init", "payload": {} })

    return proxy;
}

