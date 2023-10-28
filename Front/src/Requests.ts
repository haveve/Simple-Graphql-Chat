import {ajax} from 'rxjs/ajax'
import { webSocket } from 'rxjs/webSocket'
import { Message } from './Types';

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

export function ConnectToChat(){
    const socket = webSocket<defaultSubscriptionResponse<any>>({
        url:`wss://localhost:7000/graphql`,
        protocol: 'graphql-ws'
    })
    socket.next({ "type": "connection_init", "payload": {} })
    return socket;
}