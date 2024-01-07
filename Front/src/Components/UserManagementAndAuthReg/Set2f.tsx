import React, { Dispatch, SetStateAction, useEffect, useState } from 'react';
import { Form, Button, Card, Modal, Row, Col, ProgressBar, InputGroup, ListGroup, Image } from "react-bootstrap";
import { _2fAuthResult, axajSetUser2fAuth } from '../../Requests/AuthorizationRequests';
import { useTypedDispatch, useTypedSelector } from '../../Redux/store';
import { addUser } from '../../Redux/Slicers/UserSlicer';
import GetElementInfDueToState from '../GetElementDueToState';
import { sliceState as state } from '../../Redux/Slicers/AuthRegSlicer';
import { useTranslation } from 'react-i18next';

export default function Set2factorAuth(props: { isVisibleSet2fa: boolean, setVisibleSet2fa: (v: boolean) => void, _2fAuthData: _2fAuthResult | null }) {

    const { _2fAuthData, isVisibleSet2fa, setVisibleSet2fa } = props;
    const [enteredCode, setEnteredCode] = useState("");
    const [state, setState] = useState<state>({ state: 'idle' });

    const dispatch = useTypedDispatch();
    const user = useTypedSelector(store => store.user.user);

    const { t } = useTranslation();

    return <>
        <Modal
            show={isVisibleSet2fa}
            onHide={() => {
                setState({ state: 'idle' })
                setVisibleSet2fa(false)
            }}
            size='lg'
            centered>
            <Modal.Header closeButton className='h2'>{t('Set2f')}</Modal.Header>
            <Modal.Body>
                <Col className="d-flex  flex-row  ">
                    <Col>
                        <div className='h5'>{t('QrCode')}</div>
                        <Image thumbnail src={_2fAuthData?.qrUrl}></Image>
                    </Col>
                    <Col sm={8} className='ms-3'>
                        <div className='h5'>{t('ManualCode')}</div>
                        <p><em className="autoWordSpace">{_2fAuthData?.manualEntry}</em></p>
                        <GetElementInfDueToState state={state.state} message={state.message} />
                    </Col>
                </Col>
            </Modal.Body>
            <Modal.Footer className='d-flex flex-row justify-content-between'>
                <div className='w-50'>
                    <Form.Control
                        placeholder={t('OneTimeCodePlaceholder')}
                        onChange={(event) => setEnteredCode(event.target.value)}>
                    </Form.Control>
                </div>
                <Button variant='outline-warning' className='w-25'
                    onClick={() => {
                        setState({ state: 'pending' })
                        axajSetUser2fAuth(_2fAuthData!.key, enteredCode).subscribe({
                            next: () => {
                                setState({ message: t('DefaultSuccessMessage'), state: 'success' })
                                dispatch(addUser({ ...(user!), key2Auth: "key" }));
                            },
                            error: (error) => {
                                console.log(error)
                                setState({ message: t('IncorrectOneTimeCode'), state: 'error' })
                            }
                        })
                    }}
                >{t('Set2f')}</Button>
            </Modal.Footer>
        </Modal >
    </>
}