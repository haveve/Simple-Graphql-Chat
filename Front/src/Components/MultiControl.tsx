import React, { useState } from 'react';
import { IsWhiteSpaceOrEmpty } from '../Features/Functions';
import TextareaAutosize from 'react-textarea-autosize';
import '../Styles/App.css'

export default function MultiControl(props: { size?:'w-25'|'w-50'|'w-100',placeHolder?:string,className?:string,SendMessage: (createdMessage: string, setCreatedMessage: React.Dispatch<string>) => void }) {
    const { SendMessage,size,placeHolder,className } = props;
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

    return <div className='d-inline-flex flex-row justify-content-center w-100 mb-2'>
        <div className={`${size?size:'w-50'} send-message-container ${className}`}>
            <TextareaAutosize onChange={changeEventHandler} placeholder={placeHolder?placeHolder:'Message'} value={createdMessage} className='send-message-input' onKeyDown={keyDownEventHandler}></TextareaAutosize>
        </div>
    </div>
}