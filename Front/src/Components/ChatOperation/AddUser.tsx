import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { addUserToChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import type { ChatUpdate } from '../../Features/Types';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import ChatHeader from '../ChatHeader';
import { shallowEqual } from 'react-redux';

export default function AddUserToChat(props: { chatId: number | null, show: boolean, setShow: (value: boolean) => void }) {

    const { show, setShow, chatId } = props;
    const [addedName, setAddedName] = useState("");

    const chat = useTypedSelector(store => store.chat.chats.find(el => el.id === chatId))

    const closeHandler = () => {
        setShow(false)
        setAddedName("")
    };
    const updateChatHandler = () => {
        if (chat) {
            const connection = ConnectToChat()
            connection.subscribe((sub) => sub.next(
                RequestBuilder('start',
                    { query: addUserToChatMutation, variables: { chatId,user:addedName } })))
        }
    }
    const updateNameHandler: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setAddedName(event.target.value)
    }

    return <Modal centered show={show}>
        <Modal.Body>
            {chat ? <ChatHeader currentChat={{...chat!,chatMembersCount:0}} withoutParticipants = {true} onlyIco = {true} /> : null}
            <Form.Control onChange={updateNameHandler} size='lg' placeholder='User nick name or email' className='border border-dark mt-3'></Form.Control>
            <div className='d-flex justify-content-end gap-3 mt-3'><Button variant='primary' size='lg' onClick={closeHandler}>Cancel</Button>
                <Button variant='success' size='lg' onClick={updateChatHandler}>Add</Button></div>
        </Modal.Body>
    </Modal>
}
