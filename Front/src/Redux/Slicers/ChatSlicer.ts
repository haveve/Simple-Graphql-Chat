import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import { FullChat, Chat, ReduxMessage, Message, ChatParticipant } from '../../Features/Types'
import { GetStringFromDateTime, SetMessageId, SortByOnline } from '../../Features/Functions'
import randomColor from 'randomcolor'
import { ParticipantState } from '../../Features/Types'
import { baseChatPictureFolder, baseMessageFolder } from '../../Features/Constants'
import { IsWhiteSpaceOrEmpty } from '../../Features/Functions'
import { GetRelativePathFromUserAvatarName } from './UserSlicer'

export type Status = 'error' | 'idle' | 'pending' | 'success'

export type ReduxCurrentChat = FullChat & { color: string };
export type ReduxChat = Chat & { color: string };
export type ReduxParticipant = ChatParticipant & { color: string }

export type sliceState = {
    currentChat: ReduxCurrentChat | null,
    messages: ReduxMessage[],
    status: Status,
    participants: ReduxParticipant[],
    chats: ReduxChat[],
    error?: string,
    updatedMessage?: ReduxMessage,
    maxMessageHistoryFetchDate?: string,
    noHistoryMessagesLost?: boolean,
    imageToScaledShow?: string,
}

export type DeleteAll = {
    chatId: number,
    fromId: number
}

export enum ChangeParticipantsType {
    DELETE,
    ADD
}

const initialState: sliceState = {
    currentChat: null,
    participants: [],
    messages: [],
    chats: [],
    status: 'idle'
}

export function GetRelativePathFromChatAvatarName(avatar: string) {
    return baseChatPictureFolder + '/' + avatar;
}

export function GetRelativePathFromMessagePictureName(chatId: number, avatar: string) {
    return baseMessageFolder + '/' + chatId + '/' + avatar;
}

export const chatSlicer = createSlice({
    name: "chat",
    initialState,
    reducers: {
        setChat: {
            reducer: (state, action: PayloadAction<FullChat>) => {
                state.currentChat = {
                    ...action.payload, color: action.payload.avatar ? '' : state.chats.find(el => el.id === action.payload.id)!.color,
                    avatar: action.payload.avatar ? GetRelativePathFromChatAvatarName(action.payload.avatar) : null
                }
                state.updatedMessage = undefined;
                state.maxMessageHistoryFetchDate = undefined;
                state.noHistoryMessagesLost = undefined;
                state.messages = [];
                state.status = 'idle'
            },
            prepare: (payload: FullChat) => {
                return { payload: { ...payload } }
            }
        },

        setChats: {
            reducer: (state, action: PayloadAction<ReduxChat[]>) => {
                state.chats = action.payload
                state.status = 'idle'
            },
            prepare: (payload: Chat[]) => {
                return {
                    payload: payload.map(el => ({
                        ...el,
                        color: el.avatar ? '' : randomColor({ hue: 'red', luminosity: 'light' }),
                        avatar: el.avatar ? GetRelativePathFromChatAvatarName(el.avatar) : null
                    }))
                }
            }
        },

        dropCurrentChat: (state) => {
            state.status = 'idle'
            state.currentChat = null;
        },

        addChat: (state, action: PayloadAction<Chat>) => {
            state.status = 'idle'
            state.chats.unshift({
                ...action.payload, color: action.payload.avatar ? '' : randomColor({ hue: 'red', luminosity: 'light' }),
                avatar: action.payload.avatar ? GetRelativePathFromChatAvatarName(action.payload.avatar) : undefined
            });
        },

        updateChat(state, action: PayloadAction<Chat>) {
            state.status = 'idle'
            state.chats = state.chats.map(el => {
                if (el.id === action.payload.id) {
                    return {
                        ...el, color: el.color, name: IsWhiteSpaceOrEmpty(action.payload.name) ? el.name : action.payload.name,
                        avatar: action.payload.avatar ? GetRelativePathFromChatAvatarName(action.payload.avatar) : el.avatar
                    }
                }
                return el
            })
            if (state.currentChat?.id === action.payload.id) {
                state.currentChat.name = IsWhiteSpaceOrEmpty(action.payload.name) ? state.currentChat.name : action.payload.name;
                if (action.payload.avatar) {
                    state.currentChat.avatar = GetRelativePathFromChatAvatarName(action.payload.avatar);
                }
            }
        },

        removeChat: (state, action: PayloadAction<number>) => {
            state.status = 'idle'
            state.chats = state.chats.filter(el => el.id !== action.payload)
            if (state.currentChat?.id === action.payload) {
                state.currentChat = null;
            }
        },

        setMessages: {
            reducer: (state, action: PayloadAction<ReduxMessage[]>) => {
                state.status = 'idle'

                const length = action.payload.length;
                if (!state.messages.length && length) {
                    state.maxMessageHistoryFetchDate = action.payload[length - 1].sentAt
                }

                if (!length) {
                    state.noHistoryMessagesLost = true;
                    return;
                }

                const reversed = [...action.payload].reverse()
                state.messages = [...reversed, ...state.messages]
            },
            prepare: (action: Message[]) => {
                return {
                    payload: action.map<ReduxMessage>(el => {
                        let message = {
                            ...el, sentAt: GetStringFromDateTime(el.sentAt),
                            image: el.image ? GetRelativePathFromMessagePictureName(el.chatId, el.image) : null
                        }
                        SetMessageId(message)
                        return message
                    })
                }
            }
        },

        setParticipants: {
            reducer: (state, action: PayloadAction<ReduxParticipant[]>) => {
                state.status = 'idle'
                state.participants = [...action.payload]
            },
            prepare: (payload: ChatParticipant[]) => {
                return {
                    payload:
                        SortByOnline(payload.map(el =>
                        ({
                            ...el,
                            color: el.avatar ? '' : randomColor({ hue: 'blue', luminosity: 'light' }),
                            avatar: el.avatar ? GetRelativePathFromUserAvatarName(el.avatar) : null
                        })
                        )) as ReduxParticipant[]
                }
            }
        },

        addMessage: {
            reducer: (state, action: PayloadAction<ReduxMessage>) => {
                state.status = 'idle'
                state.messages.push(action.payload);
            },
            prepare: (payload: Message) => {
                let message = {
                    ...payload,
                    sentAt: GetStringFromDateTime(payload.sentAt),
                    image: payload.image ? GetRelativePathFromMessagePictureName(payload.chatId, payload.image) : null
                }

                SetMessageId(message)
                return {
                    payload: message
                }
            }
        },

        deleteAll: (state, action: PayloadAction<DeleteAll>) => {
            if (state.currentChat?.id === action.payload.chatId) {
                state.messages = state.messages.filter(message => message.fromId !== action.payload.fromId)
            }
        },

        removeMessage: (state, action: PayloadAction<string>) => {
            state.status = 'idle'
            state.messages = state.messages.filter(el => el.id! !== action.payload)
        },

        updateMessage(state, action: PayloadAction<Message>) {
            state.status = 'idle'
            state.messages = state.messages.map(el => {
                if (el.id === action.payload.id) {
                    return { ...el, content: action.payload.content }
                }
                return el;
            });
        },

        changeChatParticipants(state, action: PayloadAction<ChangeParticipantsType>) {
            state.status = 'idle'
            if (!state.currentChat) {
                return;
            }

            switch (action.payload) {
                case ChangeParticipantsType.DELETE:
                    state.currentChat.chatMembersCount = state.currentChat.chatMembersCount - 1;
                    break;
                case ChangeParticipantsType.ADD:
                    state.currentChat.chatMembersCount = state.currentChat.chatMembersCount + 1;
                    break;
            }
        },

        setError(state, action: PayloadAction<string>) {
            state.status = 'error'
            state.error = action.payload
        },

        setState(state, action: PayloadAction<Status>) {
            state.status = action.payload
        },

        setParticipantState(state, action: PayloadAction<ParticipantState>) {
            if (action.payload.nickName) {
                state.messages = state.messages.map(el => {
                    if (el.fromId === action.payload.id) {
                        return { ...el, nickName: action.payload.nickName! }
                    }
                    return el;
                })
            }

            state.participants = SortByOnline(state.participants.map(el => {
                if (el.id === action.payload.id) {
                    return { ...el, online: action.payload.online }
                }
                return el
            })) as ReduxParticipant[]
        },
        setUpdateMessage(state, action: PayloadAction<ReduxMessage>) {
            state.updatedMessage = action.payload
        },
        dropUpdateMessage(state) {
            state.updatedMessage = undefined
        },

        setImageToScaledShow(state, action: PayloadAction<string>) {
            state.imageToScaledShow = action.payload;
        },

        dropImageToScaledShow(state) {
            state.imageToScaledShow = undefined;
        }

    }
})

export default chatSlicer.reducer;
export const { setUpdateMessage, dropUpdateMessage, updateMessage, setParticipantState, deleteAll, setError, setState, removeMessage, setChats, changeChatParticipants, setChat, setMessages, addMessage, updateChat, setParticipants, addChat, removeChat, dropCurrentChat, setImageToScaledShow, dropImageToScaledShow } = chatSlicer.actions;