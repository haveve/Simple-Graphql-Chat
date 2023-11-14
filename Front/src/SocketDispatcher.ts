import { defaultSubscriptionResponse, subscriptionDataType, AllFieldsRequestType } from './Requests/Requests';
import { ChatNotificationType, ChatResultType, Message, MessageType } from './Features/Types';
import store from './Redux/store';
import { updateMessage, removeMessage, changeCurrentChatParticipances,setChats, setMessages, addMessage, updateChat, setParticipants, addChat, removeChat } from './Redux/Slicers/ChatSlicer';
import { SetMessageId } from './Features/Functions';
import { batch } from 'react-redux'
import { addUser } from './Redux/Slicers/UserSlicer';

const dispatch = store.dispatch

export type DispatchReturnType<T> = {
    dataType: string | null
    data?: T
}

export default function Dispatch(response: defaultSubscriptionResponse<any>) {
    switch (response.type) {
        case 'connection_ack':
            console.log('connection ack');
            break;
        case 'complete':
            console.log('sub completed');
            break;
        case 'data':
            const responseWithData: defaultSubscriptionResponse<subscriptionDataType<AllFieldsRequestType, any>> = response;
            const data = responseWithData.payload.data;
            if (data.addMessage) {
                let message: Message = data.addMessage;
                SetMessageId(message)
                dispatch(addMessage(message))
            }
            else if (data.messages) {
                let messages = data.messages
                messages.forEach(el => {
                    SetMessageId(el)
                })
                dispatch(setMessages(messages))
            }
            else if (data.addUserToChat) {
                alert("user was added to chat")
            }
            else if (data.removeUserFromChat) {
                alert("user was removed from chat")
            }
            else if (data.chatNotification) {
                if (data.chatNotification.typeC) {
                    const chatNotification = data.chatNotification
                    switch (chatNotification.typeC) {
                        case ChatResultType.UPDATE:
                            dispatch(updateChat(chatNotification))
                            break;
                        case ChatResultType.DELETE:
                            dispatch(removeChat(chatNotification.id))
                            break;
                    }
                }
                else {
                    const messageNotification = data.chatNotification
                    switch (messageNotification.typeM) {
                        case MessageType.CREATE:
                            dispatch(addMessage(messageNotification))
                            break;
                        case MessageType.UPDATE:
                            dispatch(updateMessage(messageNotification))
                            break;
                        case MessageType.DELETE:
                            SetMessageId(messageNotification)
                            dispatch(removeMessage(messageNotification.id))
                            break;
                        case MessageType.USER_ADD:
                            SetMessageId(messageNotification)
                            batch(() => {
                                dispatch(addMessage(messageNotification))
                                dispatch(changeCurrentChatParticipances(1))
                            })
                            break;
                        case MessageType.USER_REMOVED:
                            SetMessageId(messageNotification)
                            batch(() => {
                                dispatch(addMessage(messageNotification))
                                dispatch(changeCurrentChatParticipances(-1))
                            })
                            break;
                    }
                }
            }
            else if (data.chats){
                dispatch(setChats(data.chats))
            }
            else if (data.createChat){
                dispatch(addChat(data.createChat))
            }
            else if(data.participants){
                dispatch(setParticipants(data.participants))
            }
            else if(data.removeChat){
                dispatch(removeChat(data.removeChat))
            }
            else if (data.removeMessage){
                SetMessageId(data.removeMessage)
                dispatch(removeMessage(data.removeMessage.id!))
            }
            else if (data.removeUserFromChat){
                console.log("Ok")
            }
            else if (data.updateChat){
                dispatch(updateChat(data.updateChat))
            }
            else if(data.updateMessage){
                SetMessageId(data.updateMessage)
                dispatch(updateMessage(data.updateMessage))
            }
            else if (data.userNotification){
                switch(data.userNotification.notificationType){
                    case ChatNotificationType.ENROLL:
                        dispatch(addChat(data.userNotification))
                        break;
                    case ChatNotificationType.BANISH:
                        dispatch(removeChat(data.userNotification.id))
                        break;

                }
            }
            else if(data.user){
                dispatch(addUser(data.user))
            }
            else{
                console.log(`unknown data type:${JSON.stringify(responseWithData)}`)
            }
            
            break;
        case 'error':
            console.log('error');
    }
    console.log(JSON.stringify(response));

}
