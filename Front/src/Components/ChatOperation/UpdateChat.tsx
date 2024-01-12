import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import { updateChatMutation } from '../../Features/Queries';
import { useTypedSelector } from '../../Redux/store';
import type { ChatUpdate } from '../../Features/Types';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import { setError, setState } from '../../Redux/Slicers/ChatSlicer';
import Icon from '../Icon';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCameraAlt } from '@fortawesome/free-solid-svg-icons';
import { updateChatAvatarMutation } from '../../Features/Queries';
import { ajaxUploadFile } from '../../Requests/Requests';
import { useTranslation } from 'react-i18next';

export default function UpdateChat(props: { chatId: number | null, show: boolean, setShow: (value: boolean) => void }) {

    const chatPending = setState('pending')

    const { t } = useTranslation();

    const { show, setShow, chatId } = props;
    const [updatedName, setUpdatedName] = useState("");
    const [fileError, setFileError] = useState("");

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
                    { query: updateChatMutation, variables: { chat: updatedChat } }), chatPending))
        }
    }
    const updateNameHandler: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setUpdatedName(event.target.value)
    }

    return <Modal centered show={show}>
        <Modal.Body>
            {chat ? <><Icon name={chat.name} color={chat.color} onlyImage={true} onlyImageSmall={true} src={chat.avatar} children={
                <label htmlFor="selec-file-avatar" className='selec-file-avatar d-flex justify-content-center align-items-center h-100 w-100'>
                    <FontAwesomeIcon icon={faCameraAlt}></FontAwesomeIcon>
                </label>
            } /> <span className='h5 ms-2'>{chat.name}</span> </> : null}
            <span className='text-danger'>{fileError}</span>
            <Form.Control onChange={updateNameHandler} size='lg' placeholder={t('UpdateChat.NewChatName')} className='border border-dark mt-3'></Form.Control>
            <div className='d-flex justify-content-end gap-3 mt-3'><Button variant='primary' size='lg' onClick={closeHandler}>{t('Cancel')}</Button>
                <Button variant='success' size='lg' onClick={updateChatHandler}>{t('Update')}</Button></div>
            <input type="file" accept="image/*" hidden id="selec-file-avatar" onChange={event => {
                if (event.target.validity.valid && event.target.files && event.target.files[0]) {
                    const img = event.target.files[0]
                    try {
                        ajaxUploadFile(img, "file", updateChatAvatarMutation, { chatId: chatId })
                            .subscribe({
                                next: (data) => {
                                    setFileError("")
                                },
                                error: (error) => {
                                    setFileError(t('DefaultErrorMessage'))
                                }
                            })
                    }
                    catch (error) {
                        const strError = error as string
                        if (strError)
                            setFileError(strError)
                        else
                            console.log(JSON.stringify(error));
                    }
                }
            }} />
        </Modal.Body>
    </Modal>
}
