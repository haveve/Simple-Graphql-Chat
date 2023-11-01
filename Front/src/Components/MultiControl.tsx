import React, { useState } from 'react';
import { IsWhiteSpaceOrEmpty } from '../Features/Functions';
import TextareaAutosize from 'react-textarea-autosize';
import '../Styles/App.css'

export default function MultiControl(props: { SendMessage: (createdMessage: string, setCreatedMessage: React.Dispatch<string>) => void }) {
    const { SendMessage } = props;
    const [createdMessage, setCreatedMessage] = useState('');

    const keyDownEventHandler = (event: React.KeyboardEvent<HTMLTextAreaElement>) => {
        if (!event.shiftKey && event.key === 'Enter' && !IsWhiteSpaceOrEmpty(createdMessage)) {
            SendMessage(createdMessage, setCreatedMessage)
            setCreatedMessage('')
        }
    }
    const changeEventHandler = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
        if(IsWhiteSpaceOrEmpty(event.target.value)&&IsWhiteSpaceOrEmpty(createdMessage)){
            return;
        }
        setCreatedMessage(event.target.value);
    }

    return <div className='d-inline-flex p-3 flex-row justify-content-center'>
        <div className='w-50 send-message-container'>
            <TextareaAutosize onChange={changeEventHandler} placeholder='Message' value={createdMessage} className='send-message-input' onKeyDown={keyDownEventHandler}></TextareaAutosize>
        </div>
    </div>
}