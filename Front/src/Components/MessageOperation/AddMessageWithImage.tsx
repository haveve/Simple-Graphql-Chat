import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import MultiControl from '../MultiControl';
import { maxSymbolsInMessage } from '../Chat';
import { useTranslation } from 'react-i18next';

export default function AddMessageWithImage(props: { file: File, show: boolean, message: string, setMessage: (message: string) => void, setShow: (value: boolean) => void, handleSubmit: (message: string, file: File) => void }) {

    const { show, setShow, file, message, setMessage, handleSubmit } = props;
    const [imgUrl, setImgUrl] = useState('')
    const { t } = useTranslation();

    useEffect(() => {
        setImgUrl(URL.createObjectURL(file))
    }, [file.name, file.size, file.type])


    const closeHandler = () => {
        setShow(false)
    };


    return <Modal centered show={show}>
        <Modal.Header className='h2' closeButton onHide={closeHandler}>{t("SendPicture")}</Modal.Header>
        < Modal.Body className='d-flex justify-content-center mx-3'>
            <div className='send-icon' style={
                {
                    backgroundImage: `url(${imgUrl})`,
                    backgroundRepeat: 'no-repeat',
                    backgroundPosition: 'center',
                    backgroundSize: 'contain'
                }
            }>
            </div>
        </Modal.Body >
        <div className='px-4'>
            <MultiControl size='w-100' value={message} parentState={{ setState: setMessage, state: message }} maxSymbols={maxSymbolsInMessage} SendMessage={(createdMessage) => {
                setShow(false)
                handleSubmit(createdMessage, file)
            }} />
        </div>
    </Modal >
}
