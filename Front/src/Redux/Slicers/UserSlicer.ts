import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import { Chat, ReduxMessage, Message, ChatParticipant, User } from '../../Features/Types'
import { Status } from './ChatSlicer'

export type UserInitialType = {
    user: User | null,
    status: Status
    error?: string
}

const initialState: UserInitialType = {
    user: null,
    status: 'idle',
}

const userSlicer = createSlice({
    name: 'user',
    initialState,
    reducers: {
        addUser: (state, action: PayloadAction<User>) => {
            state.user = { ...action.payload }
        },

        setError(state, action: PayloadAction<string>) {
            state.error = action.payload
        },

        setState(state, action: PayloadAction<Status>) {
            state.status = action.payload
        }
    }
})

export default userSlicer.reducer;
export const { addUser, setState, setError } = userSlicer.actions;