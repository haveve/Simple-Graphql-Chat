
import React, { useState, useEffect, useRef } from 'react';
import distinctColors from 'distinct-colors'
import { GetAbbreviationFromPhrase, GetDisplayedName } from '../Features/Functions';

export default function ChatSelect() {

    const chatText = "My Awesome Chat Very Good For Me";
    const [selectedChat, setSelectedChat] = useState(0);
    const distinctC = distinctColors({ count: 2 });

    return <>
        <div onClick={() => setSelectedChat(0)} className={`${selectedChat === 0 ? 'selected' : null}  chat chat-hover p-2 h5`}>
            <div className='chat-icon-size' style={{
                backgroundColor: distinctC[0].css()
            }}>{GetAbbreviationFromPhrase(chatText)}</div>
            <span className={`ps-2`}>{GetDisplayedName(chatText)}</span>
        </div>
    </>
}