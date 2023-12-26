import React from 'react';
import {Alert, Modal, Button, Col, Row} from "react-bootstrap";
import {MessageType} from './NotificationModalWindow';

export default function CheckModalWindow(props: {
    innerText: string,
    isShowed: boolean,
    dropMessage: (message: string) => void,
    messageType: MessageType,
    agree: () => void,
    reject: () => void
}) {

    let pict = <svg></svg>;
    let color = "";

    switch (props.messageType) {
        case MessageType.Error:
            color = "danger"
            pict = <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor"
                        className="bi bi-exclamation-octagon" viewBox="0 0 16 16">
                <path
                    d="M4.54.146A.5.5 0 0 1 4.893 0h6.214a.5.5 0 0 1 .353.146l4.394 4.394a.5.5 0 0 1 .146.353v6.214a.5.5 0 0 1-.146.353l-4.394 4.394a.5.5 0 0 1-.353.146H4.893a.5.5 0 0 1-.353-.146L.146 11.46A.5.5 0 0 1 0 11.107V4.893a.5.5 0 0 1 .146-.353L4.54.146zM5.1 1 1 5.1v5.8L5.1 15h5.8l4.1-4.1V5.1L10.9 1H5.1z"/>
                <path
                    d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z"/>
            </svg>
            break;
        case MessageType.Warning:
            color = "warning"
            pict = <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor"
                        className="bi bi-exclamation-diamond" viewBox="0 0 16 16">
                <path
                    d="M6.95.435c.58-.58 1.52-.58 2.1 0l6.515 6.516c.58.58.58 1.519 0 2.098L9.05 15.565c-.58.58-1.519.58-2.098 0L.435 9.05a1.482 1.482 0 0 1 0-2.098L6.95.435zm1.4.7a.495.495 0 0 0-.7 0L1.134 7.65a.495.495 0 0 0 0 .7l6.516 6.516a.495.495 0 0 0 .7 0l6.516-6.516a.495.495 0 0 0 0-.7L8.35 1.134z"/>
                <path
                    d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z"/>
            </svg>
            break;

        case MessageType.Success:
            color = "success"
            pict = <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor"
                        className="bi bi-emoji-sunglasses" viewBox="0 0 16 16">
                <path
                    d="M4.968 9.75a.5.5 0 1 0-.866.5A4.498 4.498 0 0 0 8 12.5a4.5 4.5 0 0 0 3.898-2.25.5.5 0 1 0-.866-.5A3.498 3.498 0 0 1 8 11.5a3.498 3.498 0 0 1-3.032-1.75zM7 5.116V5a1 1 0 0 0-1-1H3.28a1 1 0 0 0-.97 1.243l.311 1.242A2 2 0 0 0 4.561 8H5a2 2 0 0 0 1.994-1.839A2.99 2.99 0 0 1 8 6c.393 0 .74.064 1.006.161A2 2 0 0 0 11 8h.438a2 2 0 0 0 1.94-1.515l.311-1.242A1 1 0 0 0 12.72 4H10a1 1 0 0 0-1 1v.116A4.22 4.22 0 0 0 8 5c-.35 0-.69.04-1 .116z"/>
                <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-1 0A7 7 0 1 0 1 8a7 7 0 0 0 14 0z"/>
            </svg>
            break;
    }


    return <Modal
        size="xl"
        show={props.isShowed}
    >
        <Alert className='h-100 m-0 ' variant={color}>
            <Row>
                <Col xs='auto'>
                    {pict}
                </Col>
                <Col>
                    {props.innerText}
                </Col>
            </Row>
            <Row>
                <Col className='w-100 justify-content-end gap-2 d-flex '>
                    <Button variant='danger' onClick={() => {
                        props.dropMessage("")
                        props.reject();
                    }}>{"reject"}</Button>
                    <Button variant='success' onClick={() => {
                        props.dropMessage("")
                        props.agree();
                    }}>{"agree"}</Button>
                </Col>
            </Row>
        </Alert>
    </Modal>
}