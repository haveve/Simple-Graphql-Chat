import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { updateChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import type { ChatUpdate } from '../../Features/Types';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import ChatHeader from '../ChatHeader';

export default function UpdateChat(props: { chatId: number | null, show: boolean, setShow: (value: boolean) => void }) {

    const { show, setShow, chatId } = props;
    const [updatedName, setUpdatedName] = useState("");

    const chat = useTypedSelector(store => store.chat.chats.find(el => el.id === chatId))

    const closeHandler = () => {
        setShow(false)
        setUpdatedName("")
    };
    const updateChatHandler = () => {
        if (chat) {
            const updatedChat: ChatUpdate = { name: updatedName, id: chatId! }
            const connection = ConnectToChat()
            connection.subscribe((sub) => sub.next(
                RequestBuilder('start',
                    { query: updateChatMutation, variables: { chat: updatedChat } })))
        }
    }
    const updateNameHandler: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setUpdatedName(event.target.value)
    }

    return <Modal centered show={show}>
        <Modal.Body>
            {chat ? <ChatHeader currentChat={{...chat!,chatMembersCount:0}} withoutParticipants = {true} onlyIco = {true} /> : null}
            <Form.Control onChange={updateNameHandler} size='lg' placeholder='New chat name' className='border border-dark mt-3'></Form.Control>
            <div className='d-flex justify-content-end gap-3 mt-3'><Button variant='primary' size='lg' onClick={closeHandler}>Cancel</Button>
                <Button variant='success' size='lg' onClick={updateChatHandler}>Update</Button></div>
        </Modal.Body>
    </Modal>
}
