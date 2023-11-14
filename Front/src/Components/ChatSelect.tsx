import React, { useState, useEffect, useRef, useMemo } from 'react';
import distinctColors from 'distinct-colors'
import { GetAbbreviationFromPhrase, GetDisplayedName } from '../Features/Functions';
import { useTypedSelector,useTypedDispatch } from '../Redux/store';
import { setChat } from '../Redux/Slicers/ChatSlicer';

export default function ChatSelect() {

    const chatText = "My Awesome Chat Very Good For Me";
    const userChats = useTypedSelector(store => store.chat.chats)
    const currentChat = useTypedSelector(store => store.chat.currentChat)
    const distinctC = useMemo(()=>distinctColors({ count: userChats.length }),[userChats.length]);
    const dispatch = useTypedDispatch()

    return <>
    {userChats.map(el=>
        <div onClick={() => dispatch(setChat(el))} className={`${currentChat?.id === el.id ? 'selected' : null}  chat chat-hover p-2 h5`}>
            <div className='chat-icon-size' style={{
                backgroundColor: distinctC[0].css()
            }}>{GetAbbreviationFromPhrase(chatText)}</div>
            <span className={`ps-2`}>{GetDisplayedName(chatText)}</span>
        </div>
    )}
    </>
}