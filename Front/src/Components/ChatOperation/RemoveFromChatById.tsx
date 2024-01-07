import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { removeUserFromChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import ChatHeader from '../ChatHeader';
import { setState } from '../../Redux/Slicers/ChatSlicer';
import { useTranslation } from 'react-i18next';

export default function RemoveFromChatById(props: { chatId: number | null, userName: string | null, show: boolean, setShow: (value: boolean) => void }) {

    const chatPending = setState('pending')

    const { t } = useTranslation();

    const { show, setShow, userName, chatId } = props;
    const [deleteAll, setDeleteAll] = useState(false)
    const chat = useTypedSelector(store => store.chat.chats.find(el => el.id === chatId))


    const closeHandler = () => {
        setShow(false)
        setDeleteAll(false)
    };
    const removeFromChatHandler = () => {
        const connection = ConnectToChat()
        setShow(false)
        connection.subscribe((sub) => sub.next(
            RequestBuilder('start',
                { query: removeUserFromChatMutation, variables: { chatId: chatId, deleteAll, user: userName } }), chatPending))
    }

    const deleteAllHandler: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setDeleteAll(Boolean(event.target.checked))
    }
    const textElement = <div className='h4 pt-3'>{t('SureOfWilling') + ' '}<span className='text-danger'>{t('remove').toUpperCase()} </span> <span className='text-info'>{userName}</span> {' ' + t('From').toLowerCase()} <span className='text-primary'>{chat?.name}</span> ?</div>
    return chat ? <Modal centered show={show}>
        < Modal.Body className='ps-3'>
            <ChatHeader currentChat={{ ...chat!, chatMembersCount: 0 }} withoutParticipants={true} onlyIco={true} />
            {textElement}
            <Form.Check onChange={deleteAllHandler} reverse label={t('WillingDeleteAllMessages')} className='h5 pb-4 pt-1' size={17} />
            <div className='d-flex justify-content-end gap-3'><Button variant='primary' size='lg' onClick={closeHandler}>{t('Cancel')}</Button>
                <Button variant='danger' size='lg' onClick={removeFromChatHandler}>{t('Remove')}</Button></div>
        </Modal.Body >
    </Modal > : <span></span>
}
