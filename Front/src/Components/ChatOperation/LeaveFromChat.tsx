import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { leaveFromChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import ChatHeader from '../ChatHeader';
import { setState } from '../../Redux/Slicers/ChatSlicer';
import { useTranslation } from 'react-i18next';

export default function LeaveFromChat(props: { chatId: number | null, show: boolean, setShow: (value: boolean) => void }) {
    const chatPending = setState('pending');

    const { show, setShow, chatId } = props;
    const [deleteAll, setDeleteAll] = useState(false)
    const chat = useTypedSelector(store => store.chat.chats.find(el => el.id === chatId))

    const { t } = useTranslation();

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
                    { query: leaveFromChatMutation, variables: { chatId: chat.id, deleteAll } }), chatPending))
        }
    }

    const deleteAllHandler: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setDeleteAll(Boolean(event.target.checked))
    }
    const textElement = <div className='ps-3 h4 pt-3'>{t('SureOfWilling') + ' '}<span className='text-danger'>{t('Leave').toUpperCase()} </span> <span className='text-primary'>{chat?.name}</span> ?</div>

    return chat ? <Modal centered show={show}>
        < Modal.Body >
            <ChatHeader currentChat={{ ...chat!, chatMembersCount: 0 }} withoutParticipants={true} onlyIco={true} />
            {textElement}
            <Form.Check onChange={deleteAllHandler} reverse label={t('WillingDeleteAllMessages')} className='h5 ms-3 pb-3 pt-1' size={17} />
            <div className='d-flex justify-content-end gap-3'><Button variant='primary' size='lg' onClick={closeHandler}>{t('Cancel')}</Button>
                <Button variant='danger' size='lg' onClick={leaveChatHandler}>{t('Leave')}</Button></div>
        </Modal.Body >
    </Modal > : <span></span>
}
