import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import { Status } from './ChatSlicer'

export type GlobalNotifyInitialType = {
    status: Status
    error?: string
}

const initialState: GlobalNotifyInitialType = {
    status: 'idle',
}

const userSlicer = createSlice({
    name: 'global-notify',
    initialState,
    reducers: {
        setError(state, action: PayloadAction<string>) {
            state.error = action.payload
            state.status = 'error'
        },

        setState(state, action: PayloadAction<Status>) {
            state.status = action.payload
        }
    }
})

export default userSlicer.reducer;
export const { setState, setError } = userSlicer.actions;