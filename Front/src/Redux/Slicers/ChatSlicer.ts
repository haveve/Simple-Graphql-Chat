import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import { Chat, ReduxMessage, Message, ChatParticipant } from '../../Features/Types'
import { nanoid } from 'nanoid'

export type Status = 'error' | 'idle' | 'padding' | 'success'

export type sliceState = {
    currentChat: Chat | null,
    messages: ReduxMessage[],
    status: Status,
    participants: ChatParticipant[],
    chats: Chat[],
    error?:string
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
        setChat: (state, action: PayloadAction<Chat>) => {
            state.currentChat = { ...action.payload }
            state.messages = [];
        },

        setChats: (state,action:PayloadAction<Chat[]>) => {
            state.chats = action.payload
        },

        dropCurrentChat: (state) => {
            state.currentChat = null;
        },

        addChat: (state, action: PayloadAction<Chat>) => {
            state.chats.push({ ...action.payload });
        },

        updateChat(state, action: PayloadAction<Chat>) {
            state.chats = state.chats.map(el => {
                if (el.id === action.payload.id) {
                    return { ...action.payload }
                }
                return el
            })
            if (state.currentChat?.id === action.payload.id) {
                state.currentChat.chatMembersCount = action.payload.chatMembersCount;
                state.currentChat.name = action.payload.name;
            }
        },

        removeChat: (state, action: PayloadAction<number>) => {
            state.chats = state.chats.filter(el => el.id !== action.payload)
            if (state.currentChat?.id === action.payload) {
                dropCurrentChat()
            }
        },

        setMessages: {
            reducer: (state, action: PayloadAction<ReduxMessage[]>) => {
                state.messages = action.payload
            },
            prepare: (payload: Message[]) => {
                return {
                    payload: payload.map<ReduxMessage>(el => {
                        return { ...el, sentAt: el.sentAt.toISOString() }
                    })
                }
            }
        },

        setParticipants: (state, action: PayloadAction<ChatParticipant[]>) => {
            state.participants = [...action.payload]
        },

        addMessage: {
            reducer: (state, action: PayloadAction<ReduxMessage>) => {
                state.messages.push(action.payload);
            },
            prepare: (payload: Message) => {
                return {
                    payload: {
                        ...payload,
                        sentAt: payload.sentAt.toISOString(),
                        id: nanoid()
                    }
                }
            }
        },

        removeMessage: (state, action: PayloadAction<string>) => {
            state.messages = state.messages.filter(el => el.id! !== action.payload)
        },

        updateMessage(state, action: PayloadAction<Message>) {
            state.messages = state.messages.map(el => {
                if (el.id! === action.payload.id!) {
                    return { ...action.payload, sentAt: action.payload.sentAt.toISOString() }
                }
                return el;
            });
        },

        changeCurrentChatParticipances(state, action: PayloadAction<number>) {
            if (state.currentChat) {
                state.currentChat.chatMembersCount += action.payload
            }
        }

    }
})

export default chatSlicer.reducer;
export const { updateMessage, removeMessage,setChats, changeCurrentChatParticipances, setChat, setMessages, addMessage, updateChat, setParticipants, addChat, removeChat, dropCurrentChat } = chatSlicer.actions;