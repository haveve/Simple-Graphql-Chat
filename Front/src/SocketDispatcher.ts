import { defaultSubscriptionResponse, subscriptionDataType, AllFieldsRequestType, GetNewToken } from './Requests/Requests';
import { ChatNotificationType, ChatResultType, Message, MessageType } from './Features/Types';
import store from './Redux/store';
import { updateMessage, removeMessage, deleteAll, setParticipantState, changeChatParticipances, setChats, setMessages, addMessage, updateChat, setParticipants, addChat, removeChat, setError, setChat, ChangeParticipantsType } from './Redux/Slicers/ChatSlicer';
import { SetMessageId, ToNormalDate } from './Features/Functions';
import { batch } from 'react-redux'
import { addUser } from './Redux/Slicers/UserSlicer';
import { setError as setErrorGlobal } from './Redux/Slicers/GlobalNotification';
import { defaultErrorMessage } from './Features/Constants';

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
            if (responseWithData.payload.errors) {
                dispatch(setErrorGlobal(defaultErrorMessage))
                console.log(JSON.stringify(response));
                return;
            }
            if (data.addMessage) {
            }
            else if (data.messages) {
                let messages = data.messages
                dispatch(setMessages(messages))
            }
            else if (data.addUserToChat) {
                console.log("userWasAddedToChat")
            }
            else if (data.removeUserFromChat) {
                console.log("user was removed from chat")
            }
            else if (data.chatNotification) {
                if (data.chatNotification.typeC) {
                    const chatNotification = data.chatNotification
                    switch (chatNotification.typeC) {
                        case ChatResultType.UPDATE:
                            dispatch(updateChat(chatNotification))
                            break;
                        case ChatResultType.DELETE:
                            GetNewToken().subscribe(() => {
                                dispatch(removeChat(chatNotification.id))
                            })
                            break;
                    }
                }
                else if (data.chatNotification.typeM) {
                    const messageNotification = data.chatNotification
                    switch (messageNotification.typeM) {
                        case MessageType.CREATE:
                            dispatch(addMessage(messageNotification))
                            break;
                        case MessageType.UPDATE:
                            SetMessageId(messageNotification)
                            dispatch(updateMessage(messageNotification))
                            break;
                        case MessageType.DELETE:
                            SetMessageId(messageNotification)
                            dispatch(removeMessage(messageNotification.id))
                            break;
                        case MessageType.USER_ADD:
                            SetMessageId(messageNotification)
                            batch(() => {
                                dispatch(addMessage({ ...messageNotification, fromId: null }))
                                dispatch(changeChatParticipances({ type: ChangeParticipantsType.ADD, chatId: messageNotification.chatId }))
                            })
                            break;
                        case MessageType.USER_REMOVED:
                            SetMessageId(messageNotification)
                            batch(() => {
                                dispatch(addMessage({ ...messageNotification, fromId: null }))
                                dispatch(changeChatParticipances({ type: ChangeParticipantsType.DELETE, chatId: messageNotification.chatId }))
                                if (messageNotification.deleteAll)
                                    dispatch(deleteAll({ chatId: messageNotification.chatId, fromId: messageNotification.fromId! }))
                            })
                            break;
                    }
                }
                else {
                    dispatch(setParticipantState(data.chatNotification))
                }
            }
            else if (data.chats) {
                dispatch(setChats(data.chats))
            }
            else if (data.createChat) {
                GetNewToken().subscribe(() => {
                    dispatch(addChat(data.createChat!))
                });
            }
            else if (data.participants) {
                dispatch(setParticipants(data.participants))
            }
            else if (data.removeChat) {
                dispatch(removeChat(data.removeChat))
            }
            else if (data.removeMessage) {
            }
            else if (data.updateChat) {
                dispatch(updateChat(data.updateChat))
            }
            else if (data.updateMessage) {
            }
            else if (data.userNotification) {
                switch (data.userNotification.notificationType) {
                    case ChatNotificationType.ENROLL:
                        GetNewToken().subscribe(() => {
                            dispatch(addChat(data.userNotification!))
                        })
                        break;
                    case ChatNotificationType.BANISH:
                        GetNewToken().subscribe(() => {
                            dispatch(removeChat(data.userNotification!.id))
                        })
                        break;

                }
            }
            else if (data.user) {
                dispatch(addUser(data.user))
            }
            else if (data.chatFullInfo) {
                dispatch(setChat(data.chatFullInfo))
            }
            else {
                console.log(`unknown data type:${JSON.stringify(responseWithData)}`)
            }
            break;
        case 'error':
            dispatch(setErrorGlobal(defaultErrorMessage))
            console.log(JSON.stringify(response));
            break;
    }

}
