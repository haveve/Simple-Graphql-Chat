import { defaultSubscriptionResponse,subscriptionDataType,AllFieldsRequestType} from './Requests';
import { nanoid } from 'nanoid';
import { Message, } from './Futures/Types';

export type DispatchReturnType<T> = {
    dataType:string | null
    data?:T
}

export const QUERY_ALL = 'query_all';
export const ADD_MESSAGE_ONE = 'add_one';
export const RECEIVE_SUBSCRIBED_MESSAGE = 'receive_subscribed_message'

export default function Dispatch(response: defaultSubscriptionResponse<any>):DispatchReturnType<any> {
    switch (response.type) {
        case 'connection_ack':
            console.log('connection ack');
            return {dataType:null}
        case 'complete':
            console.log('sub completed');
            return {dataType:null}
        case 'data':
            const responseWithData:defaultSubscriptionResponse<subscriptionDataType<AllFieldsRequestType,any>> = response;
            if (responseWithData.payload.data.messageAdded) {
                let message: Message = responseWithData.payload.data.messageAdded;
                message.id = nanoid();
                return {data:message,dataType:RECEIVE_SUBSCRIBED_MESSAGE}
            }
            else if(responseWithData.payload.data.messages){
                let messages = responseWithData.payload.data.messages
                messages.forEach(el=>{
                    el.id = nanoid()
                })
                return {data:messages,dataType:QUERY_ALL}
            }
            break;
        case 'error':
            console.log('error');
            return {dataType:null}
    }
    console.log(JSON.stringify(response));
    return {dataType:null}

}
