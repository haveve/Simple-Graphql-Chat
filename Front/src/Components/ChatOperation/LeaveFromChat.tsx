import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { leaveFromChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import type { ChatUpdate } from '../../Features/Types';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import ChatHeader from '../ChatHeader';

export default function LeaveFromChat(props: { chatId: number | null, show: boolean, setShow: (value: boolean) => void }) {

    const { show, setShow, chatId } = props;
    const [deleteAll, setDeleteAll] = useState(false)
    const chat = useTypedSelector(store => store.chat.chats.find(el => el.id === chatId))

    const closeHandler = () => {
        setShow(false)
        setDeleteAll(false)
    };
    const leaveChatHandler = () => {
        if (chat) {
            const connection = ConnectToChat()
            setShow(false)
            connection.subscribe((sub) => sub.next(
                RequestBuilder('start',
                    { query: leaveFromChatMutation, variables: { chatId: chat.id, deleteAll } })))
        }
    }

    const deleteAllHandler:React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setDeleteAll(Boolean(event.target.checked))
    }
    const textElement = <div className='ps-3 h4 pt-3'>Are you sure that you wanna <span className='text-danger'>leave </span> <span className='text-primary'>{chat?.name}</span> chat ?</div>

    return chat ? <Modal centered show={show}>
        < Modal.Body >
            <ChatHeader currentChat={{...chat!,chatMembersCount:0}} withoutParticipants={true} onlyIco={true} />
            {textElement}
            <Form.Check onChange={deleteAllHandler} reverse label='Do you wanna delete all messages?' className='h5 ms-3 pb-3 pt-1' size={17} />
            <div className='d-flex justify-content-end gap-3'><Button variant='primary' size='lg' onClick={closeHandler}>Cancel</Button>
                <Button variant='danger' size='lg' onClick={leaveChatHandler}>Leave</Button></div>
        </Modal.Body >
    </Modal > : <span></span>
}
