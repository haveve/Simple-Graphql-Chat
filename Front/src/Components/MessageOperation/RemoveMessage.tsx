import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { removeMessageMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import { MessageInput } from '../../Features/Types';

export default function RemoveMessage(props: { chatId:number|null,messageId: string | null, show: boolean, setShow: (value: boolean) => void }) {

    const { show, setShow, messageId,chatId } = props;
    const message = useTypedSelector(store => store.chat.messages.find(el => el.id === messageId))

    const closeHandler = () => {
        setShow(false)
    };
    const removeMessageHandler = () => {
        if (message && chatId) {
            const connection = ConnectToChat()
            setShow(false)
            const messageIn:MessageInput = {
                content: message.content,
                sentAt: message.sentAt
            }
            connection.subscribe((sub) => sub.next(
                RequestBuilder('start',
                    { query: removeMessageMutation, variables: {message:messageIn, chatId} })))
        }
    }   

    const textElement = <div className='ps-3 h4 py-3'>Are you sure that you wanna <span className='text-danger'>remove </span> message</div>

    return message && chatId ? <Modal centered show={show}>
        < Modal.Body >
            {textElement}
            <div className='d-flex justify-content-end gap-3 mt-3'><Button variant='primary' size='lg' onClick={closeHandler}>Cancel</Button>
                <Button variant='danger' size='lg' onClick={removeMessageHandler}>Remove</Button></div>
        </Modal.Body >
    </Modal > : <span></span>
}
