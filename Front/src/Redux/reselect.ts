import { createSelector } from "reselect";
import { RootState } from "./store";
import { DateFromString } from "../Features/Functions";

export const selectChatIds = createSelector(
    [(state:RootState) => state.chat.chats],
    (data)=>{
       return data.map(el => el.id);
    }
)

export type MessageIdDate = {
    sentAt:string,
    id:string
}

export const selectMessageIdsWithDate = createSelector(
    [(state:RootState) => state.chat.messages],
    (data)=>{
        const result = data.map<MessageIdDate>(el => ({id:el.id!,sentAt:el.sentAt}));
       return result;
    }
)