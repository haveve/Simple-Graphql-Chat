import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import { Chat, ReduxMessage, Message, ChatParticipant, User } from '../../Features/Types'
import { Status } from './ChatSlicer'
import { baseUserPictureFolder } from '../../Features/Constants'

export type UserInitialType = {
    user: User | null,
    status: Status
    error?: string
}

const initialState: UserInitialType = {
    user: null,
    status: 'idle',
}

export type UpdateUserResult = {
    nickName: string,
    id: number
    email: string
}

export type UpdateUser =
    {
        nickName: string,
        newPassword: string,
        oldPassword: string,
        email: string
    }

export type RemoveUser =
    {
        password: string
    }

const userSlicer = createSlice({
    name: 'user',
    initialState,
    reducers: {
        addUser: (state, action: PayloadAction<User>) => {
            if (!state.user || action.payload.avatar !== state.user.avatar) {
                state.user = { ...action.payload }
                state.user.avatar = action.payload.avatar ? baseUserPictureFolder + '/' + action.payload.avatar : null
            }
            else {
                state.user = { ...action.payload }
            }
        },

        setError(state, action: PayloadAction<string>) {
            state.error = action.payload
            state.status = 'error'
        },

        setState(state, action: PayloadAction<Status>) {
            state.status = action.payload
        },

        updateUserData(state, action: PayloadAction<UpdateUserResult>) {
            state.status = 'success'
            state.user!.email = action.payload.email
            state.user!.nickName = action.payload.nickName
        },

        updateAvatar(state, action: PayloadAction<string>) {
            if (state.user) {
                state.user!.avatar = baseUserPictureFolder + '/' + action.payload
            }
        }
    }
})

export default userSlicer.reducer;
export const { addUser, setState, setError, updateUserData, updateAvatar } = userSlicer.actions;