import { createSelector } from "reselect";
import { RootState } from "./store";

export const selectChatIds = createSelector(
    [(state:RootState) => state.chat.chats],
    (data)=>{
       return data.map(el => el.id);
    }
)

export const selectMessageIds = createSelector(
    [(state:RootState) => state.chat.messages],
    (data)=>{
       return data.map(el => el.id);
    }
)