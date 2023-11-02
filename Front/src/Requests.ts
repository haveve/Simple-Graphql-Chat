import { webSocket } from 'rxjs/webSocket'
import { Message } from './Features/Types';
import { backDomain } from './Features/Constants';

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
        url:`wss://${backDomain}/graphql`,
        protocol: 'graphql-ws'
    })
    socket.next({ "type": "connection_init", "payload": {} })
    return socket;
}