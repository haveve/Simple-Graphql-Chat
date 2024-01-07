import React, { Dispatch, SetStateAction, useEffect, useState } from 'react';
import { Form, Button, Card, Modal, Row, Col, ProgressBar, InputGroup, ListGroup, Image } from "react-bootstrap";
import { _2fAuthResult, axajSetUser2fAuth } from '../../Requests/AuthorizationRequests';
import { useTypedDispatch, useTypedSelector } from '../../Redux/store';
import { RemoveUser as RemoveUserType } from '../../Redux/Slicers/UserSlicer';
import { deleteUserMutation, RequestBuilder } from '../../Features/Queries';
import { ConnectToChat } from '../../Requests/Requests';
import { minPasswordLength, maxPasswordLength } from './SetPassword';
import { SetErrorHandler } from '../../SocketDispatcher';
import { setError, setState } from '../../Redux/Slicers/UserSlicer';
import GetElementInfDueToState from '../GetElementDueToState';
import { useTranslation } from 'react-i18next';

export const minNickNameLength = 3;
export const maxNickNameLength = 75;


export default function RemoveUser(props: { isVisible: boolean, setVisible: (v: boolean) => void }) {

    const { isVisible, setVisible } = props;

    const [password, setPassword] = useState<string | null>(null);

    const { t } = useTranslation();

    const dispatch = useTypedDispatch();
    const state = useTypedSelector(store => store.user.status);
    const error = useTypedSelector(store => store.user.error);

    const message = state === 'error' ? error : state === 'success' ? t('DefaultSuccessMessage') : undefined

    const infoMessage = <div className='h5 text-center mb-3'>{t('RemoveUser.warning1')}<br />{t('RemoveUser.warning2')}<br />{t('RemoveUser.warning3')}</div>

    const passwordValidation = () => {
        if (!password)
            return false;

        return password.length >= minPasswordLength
            && password.length <= maxPasswordLength
    }

    return <>
        <Modal
            show={isVisible}
            onHide={() => {

                dispatch(setState('idle'))
                setVisible(false)
            }}
            size='lg'
            centered>
            <Modal.Header closeButton className='h2'>{t('UserSettings.removeAccount')}</Modal.Header>
            <Modal.Body>
                <Row className='mt-3 d-flex gap-3 flex-row justify-content-center'>
                    {infoMessage}
                    <div className='w-50 h5'>
                        <Form.Group controlId="validationCustom01" className='w-100'>
                            <Form.Control
                                isInvalid={!passwordValidation()}
                                type='password'
                                placeholder={t('UserSettings.enterPassPlaceholder')}
                                onChange={(event) => setPassword(event.target.value)}>
                            </Form.Control>
                            <Form.Control.Feedback type="invalid" className='text-center'>
                                {t('ValidationPasswordError')}
                            </Form.Control.Feedback>
                        </Form.Group>
                    </div>
                    <div className='d-flex flex-row justify-content-center'>
                        <Button variant='danger' type='submit' className='w-25' onClick={() => {
                            if (!passwordValidation()) {
                                return;
                            }
                            const data: RemoveUserType = {
                                password: password!,
                            }

                            const connection = ConnectToChat()
                            connection.subscribe(sub => {
                                const request = RequestBuilder('start', {
                                    query: deleteUserMutation, variables: {
                                        data
                                    }
                                });
                                SetErrorHandler.SetHandler(request.id!, () => {
                                    dispatch(setError("Uncorrect data, password is not right."))
                                })

                                sub.next(request, setState('pending'))
                            })
                        }} >{t('Remove')}</Button>
                    </div>
                    <div className='mt-3 text-center'>
                        <GetElementInfDueToState state={state} message={message} />
                    </div>
                </Row>
            </Modal.Body>
        </Modal >
    </>
}