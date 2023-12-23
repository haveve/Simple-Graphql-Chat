import { configureStore} from "@reduxjs/toolkit";
import { TypedUseSelectorHook,useDispatch,useSelector } from "react-redux";
import chat from './Slicers/ChatSlicer'
import user from './Slicers/UserSlicer'
import global_notification from "./Slicers/GlobalNotification";
import auth_reg from "./Slicers/AuthRegSlicer";

const store = configureStore({
    reducer:{
        chat,
        user,
        global_notification,
        auth_reg
    }
})

export type RootState = ReturnType<typeof store.getState>
export type Dispatch = typeof store.dispatch

export type DispatchType = () => Dispatch;

export const useTypedDispatch:DispatchType = useDispatch
export const useTypedSelector:TypedUseSelectorHook<RootState> = useSelector 

export default store;