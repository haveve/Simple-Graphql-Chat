import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { removeMessageMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import { MessageInput } from '../../Features/Types';
import { setState } from '../../Redux/Slicers/ChatSlicer';
import { useTranslation } from 'react-i18next';

export default function RemoveMessage(props: { chatId: number | null, messageId: string | null, show: boolean, setShow: (value: boolean) => void }) {

    const chatIdle = setState('pending');

    const { t } = useTranslation();

    const { show, setShow, messageId, chatId } = props;
    const message = useTypedSelector(store => store.chat.messages.find(el => el.id === messageId))

    const closeHandler = () => {
        setShow(false)
    };
    const removeMessageHandler = () => {
        if (message && chatId) {
            const connection = ConnectToChat()
            setShow(false)
            const messageIn: MessageInput = {
                content: message.content,
                sentAt: message.sentAt
            }
            connection.subscribe((sub) => sub.next(
                RequestBuilder('start',
                    { query: removeMessageMutation, variables: { message: messageIn, chatId } }), chatIdle))
        }
    }

    const textElement = <div className='ps-3 h4 py-3'>{t('SureOfWilling') + ' '}<span className='text-danger'>{t('Remove').toUpperCase()} </span> {' ' + t('Message').toLowerCase()}</div>

    return message && chatId ? <Modal centered show={show}>
        < Modal.Body >
            {textElement}
            <div className='d-flex justify-content-end gap-3 mt-3'><Button variant='primary' size='lg' onClick={closeHandler}>{t('Cancel')}</Button>
                <Button variant='danger' size='lg' onClick={removeMessageHandler}>{t('Remove')}</Button></div>
        </Modal.Body >
    </Modal > : <span></span>
}
