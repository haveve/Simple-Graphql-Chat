import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import { Status } from './ChatSlicer'

export type sliceState = {
    message?: string,
    state: Status
}

const initialState: sliceState = {
    state: 'idle'
}

export const authRegSlicer = createSlice({
    name: "chat",
    initialState,
    reducers: {
        setState(state, action: PayloadAction<sliceState>) {
            state.message = action.payload.message
            state.state = action.payload.state
        }
    }
});


export default authRegSlicer.reducer;
export const { setState } = authRegSlicer.actions;