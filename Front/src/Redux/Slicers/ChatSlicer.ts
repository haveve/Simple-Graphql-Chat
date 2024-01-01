import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import { FullChat, Chat, ReduxMessage, Message, ChatParticipant } from '../../Features/Types'
import { GetStringFromDateTime, SetMessageId, SortByOnline } from '../../Features/Functions'
import randomColor from 'randomcolor'
import { ParticipantState } from '../../Features/Types'

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
    updatedMessage?: ReduxMessage
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


export const chatSlicer = createSlice({
    name: "chat",
    initialState,
    reducers: {
        setChat: {
            reducer: (state, action: PayloadAction<FullChat>) => {
                state.currentChat = { ...action.payload, color: state.chats.find(el => el.id === action.payload.id)!.color }
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
                return { payload: payload.map(el => ({ ...el, color: randomColor({ hue: 'red', luminosity: 'light' }) })) }
            }
        },

        dropCurrentChat: (state) => {
            state.status = 'idle'
            state.currentChat = null;
        },

        addChat: (state, action: PayloadAction<Chat>) => {
            state.status = 'idle'
            state.chats.unshift({ ...action.payload, color: randomColor({ hue: 'red', luminosity: 'light' }) });
        },

        updateChat(state, action: PayloadAction<Chat>) {
            state.status = 'idle'
            state.chats = state.chats.map(el => {
                if (el.id === action.payload.id) {
                    return { ...el, color: el.color, name: action.payload.name }
                }
                return el
            })
            if (state.currentChat?.id === action.payload.id) {
                state.currentChat.name = action.payload.name;
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
                state.messages = action.payload
            },
            prepare: (action: Message[]) => {
                return {
                    payload: action.map<ReduxMessage>(el => {
                        let message = { ...el, sentAt: GetStringFromDateTime(el.sentAt) }
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
                            color: randomColor({
                                hue: 'blue', luminosity: 'light'
                            })
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
        }

    }
})

export default chatSlicer.reducer;
export const { setUpdateMessage, dropUpdateMessage, updateMessage, setParticipantState, deleteAll, setError, setState, removeMessage, setChats, changeChatParticipants, setChat, setMessages, addMessage, updateChat, setParticipants, addChat, removeChat, dropCurrentChat } = chatSlicer.actions;