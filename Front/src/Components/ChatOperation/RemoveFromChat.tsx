import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { removeUserFromChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import { queryParticipants } from '../../Features/Queries';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import ChatHeader from '../ChatHeader';
import { IsWhiteSpaceOrEmpty } from '../../Features/Functions';
import { setState } from '../../Redux/Slicers/ChatSlicer';
import { useTranslation } from 'react-i18next';

export default function RemoveFromChat(props: { chatId: number | null, show: boolean, setShow: (value: boolean) => void }) {

    const chatPending = setState('pending')

    const { t } = useTranslation();

    const { show, setShow, chatId } = props;
    const [deleteAll, setDeleteAll] = useState(false)
    const [removeName, setRemoveName] = useState<string>("")
    const chat = useTypedSelector(store => store.chat.chats.find(el => el.id === chatId))
    const participats = useTypedSelector(store => store.chat.participants)

    useEffect(() => {
        if (chatId && show) {
            const connection = ConnectToChat();

            const request = RequestBuilder('start', { query: queryParticipants, variables: { chatId: chatId } });
            connection.subscribe(sub => {
                sub.next(request, chatPending)
            })

            return () => {
                connection.subscribe(sub => {
                    sub.next(RequestBuilder('stop', {}, request.id!), chatPending)
                })
            }
        }
    }, [chatId, show])

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
                    { query: removeUserFromChatMutation, variables: { chatId: chat.id, deleteAll, user: removeName } }), chatPending))
        }
    }

    const deleteAllHandler: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setDeleteAll(Boolean(event.target.checked))
    }

    return chat ? <Modal centered show={show}>
        < Modal.Body className='ps-3'>
            <ChatHeader currentChat={{ ...chat!, chatMembersCount: 0 }} withoutParticipants={true} onlyIco={true} />
            <Form.Select size='lg' className='mt-3' onChange={(event) => {
                setRemoveName(event.target.value)
            }}>
                <option value="">{t('SelectUser')}</option>
                {participats.map(el => {
                    if (el.id === chat.creatorId) {
                        return <option key={el.id} value={""} className='text-danger'>{el.nickName}</option>
                    }
                    return <option key={el.id} value={el.nickName}>{el.nickName}</option>
                })}
            </Form.Select>
            <Form.Check onChange={deleteAllHandler} reverse label={t('WillingDeleteAllMessages')} className='h5 pb-4 pt-1' size={17} />
            <div className='d-flex justify-content-end gap-3'><Button variant='primary' size='lg' onClick={closeHandler}>{t('Cancel')}</Button>
                <Button variant='danger' size='lg' onClick={leaveChatHandler}>{t('Remove')}</Button></div>
        </Modal.Body >
    </Modal > : <span></span>
}
