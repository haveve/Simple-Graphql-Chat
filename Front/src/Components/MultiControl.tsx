import React, { ReactElement, useEffect, useRef, useState } from 'react';
import { IsWhiteSpaceOrEmpty } from '../Features/Functions';
import TextareaAutosize from 'react-textarea-autosize';
import { useTranslation } from 'react-i18next';
import { Col } from 'react-bootstrap';

export type ParentState = {
    setState: (data: string) => void,
    state: string
}

export default function MultiControl(props: { value?: string, size?: 'w-25' | 'w-50' | 'w-100', placeHolder?: string, maxSymbols?: number, className?: string, children?: ReactElement, SendMessage: (createdMessage: string) => void, parentState?: ParentState }) {
    const { SendMessage, size, placeHolder, className, value, maxSymbols, children, parentState } = props;
    const [createdMessage, setCreatedMessage] = useState('');

    const { t } = useTranslation()

    useEffect(() => {
        if (parentState) {
            parentState.setState(value ?? '')
        } else {
            setCreatedMessage(value ?? '')
        }
    }, [value])

    const keyDownEventHandler = (event: React.KeyboardEvent<HTMLTextAreaElement>) => {
        if (!parentState && !IsWhiteSpaceOrEmpty(createdMessage) && (!maxSymbols || createdMessage.length <= maxSymbols) && !event.shiftKey && event.key === 'Enter') {
            SendMessage(createdMessage)
            setCreatedMessage('')
        } else if (parentState && !IsWhiteSpaceOrEmpty(parentState.state) && (!maxSymbols || parentState.state.length <= maxSymbols) && !event.shiftKey && event.key === 'Enter') {
            SendMessage(parentState.state)
            parentState.setState('')
        }
    }
    const changeEventHandler = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
        if (!parentState && (!IsWhiteSpaceOrEmpty(event.target.value) || !IsWhiteSpaceOrEmpty(createdMessage))) {
            setCreatedMessage(event.target.value);
            return;
        }

        if (parentState && (!IsWhiteSpaceOrEmpty(event.target.value) || !IsWhiteSpaceOrEmpty(parentState.state))) {
            parentState.setState(event.target.value);
            return;
        }
    }
    const ValidateMax = () => {
        return (maxSymbols && ((!parentState && maxSymbols - createdMessage.length < 0) || (parentState && maxSymbols - parentState.state.length < 0)) ? maxSymbols - createdMessage.length - (parentState?.state?.length ?? 0) : null)
    }

    return <div className='d-inline-flex flex-row justify-content-center w-100 mb-2'>
        <div className={`${size ? size : 'w-50'} d-flex justify-content-center send-message-container ${className}`}>
            <Col>
                <TextareaAutosize onChange={changeEventHandler} placeholder={placeHolder ? placeHolder : t('Message')} value={parentState ? parentState.state : createdMessage} className='send-message-input' onKeyDown={keyDownEventHandler}>
                </TextareaAutosize>
                <b className='text-danger'>{ValidateMax()}</b>
            </Col>
            <Col sm="auto" className='d-flex align-items-end'>
                <div>
                    {children}
                </div>
            </Col>
        </div>
    </div>
}