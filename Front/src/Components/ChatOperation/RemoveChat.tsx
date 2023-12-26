import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { removeChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import type { ChatUpdate } from '../../Features/Types';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import ChatHeader from '../ChatHeader';
import { setState } from '../../Redux/Slicers/ChatSlicer';

export default function RemoveChat(props: { chatId: number | null, show: boolean, setShow: (value: boolean) => void }) {

    const chatPending = setState('pending')

    const { show, setShow, chatId } = props;
    const chat = useTypedSelector(store => store.chat.chats.find(el => el.id === chatId))

    const closeHandler = () => {
        setShow(false)
    };
    const removeChatHandler = () => {
        if (chat) {
            const connection = ConnectToChat()
            setShow(false)
            connection.subscribe((sub) => sub.next(
                RequestBuilder('start',
                    { query: removeChatMutation, variables: { chatId: chat.id } }),chatPending))
        }
    }

    const textElement = <div className='ps-3 h4 py-3'>Are you sure that you wanna <span className='text-danger'>remove </span> <span className='text-primary'>{chat?.name}</span> chat ?</div>

    return chat ? <Modal  centered show={show}>
        < Modal.Body >
            <ChatHeader currentChat={{...chat!,chatMembersCount:0}} withoutParticipants={true} onlyIco={true} />
            {textElement}
            <div className='d-flex justify-content-end gap-3 mt-3'><Button variant='primary' size='lg' onClick={closeHandler}>Cancel</Button>
                <Button variant='danger' size='lg' onClick={removeChatHandler}>Remove</Button></div>
        </Modal.Body >
    </Modal > : <span></span>
}
