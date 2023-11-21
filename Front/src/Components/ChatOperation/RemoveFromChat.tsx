import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { removeUserFromChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import { queryParticipants } from '../../Features/Queries';
import type { ChatUpdate } from '../../Features/Types';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import ChatHeader from '../ChatHeader';
import { IsWhiteSpaceOrEmpty } from '../../Features/Functions';

export default function RemoveFromChat(props: { chatId: number | null, show: boolean, setShow: (value: boolean) => void }) {

    const { show, setShow, chatId } = props;
    const [deleteAll, setDeleteAll] = useState(false)
    const [removeName, setRemoveName] = useState<string>("")
    const chat = useTypedSelector(store => store.chat.chats.find(el => el.id === chatId))
    const participats = useTypedSelector(store => store.chat.participants)

    useEffect(() => {
        if (chatId != null && show) {
            const connection = ConnectToChat();
            connection.subscribe(sub => {
                sub.next(RequestBuilder('start', { query: queryParticipants, variables: { chatId: chatId } }))
            })
        }
    }, [chatId,show])

    const closeHandler = () => {
        setShow(false)
        setDeleteAll(false)
        setRemoveName("")
    };
    const leaveChatHandler = () => {
        if (chat && !IsWhiteSpaceOrEmpty(removeName)) {
            const connection = ConnectToChat()
            setShow(false)
            connection.subscribe((sub) => sub.next(
                RequestBuilder('start',
                    { query: removeUserFromChatMutation, variables: { chatId: chat.id, deleteAll, user: removeName } })))
        }
    }

    const deleteAllHandler: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setDeleteAll(Boolean(event.target.checked))
    }
    const textElement = <div className='h4 pt-3'>Are you sure that you wanna <span className='text-danger'>leave </span> <span className='text-primary'>{chat?.name}</span> chat ?</div>
    return chat ? <Modal centered show={show}>
        < Modal.Body className='ps-3'>
            <ChatHeader currentChat={{...chat!,chatMembersCount:0}} withoutParticipants={true} onlyIco={true} />
            {textElement}
            <Form.Select size='lg' className='mt-3' onChange={(event) => {
                setRemoveName(event.target.value)
            }}>
                    <option value = "">select user</option>
                {participats.map(el =>
                    <option value={el.nickName}>{el.nickName}</option>)}
            </Form.Select>
            <Form.Check onChange={deleteAllHandler} reverse label='Do you wanna delete all messages?' className='h5 pb-4 pt-1' size={17} />
            <div className='d-flex justify-content-end gap-3'><Button variant='primary' size='lg' onClick={closeHandler}>Cancel</Button>
                <Button variant='danger' size='lg' onClick={leaveChatHandler}>Remove</Button></div>
        </Modal.Body >
    </Modal > : <span></span>
}
