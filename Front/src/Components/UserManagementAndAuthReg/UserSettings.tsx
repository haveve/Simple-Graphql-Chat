import React, { Dispatch, SetStateAction, useEffect, useState } from 'react';
import { Form, Button, Modal, Row, Col, } from "react-bootstrap";
import { _2fAuthResult } from '../../Requests/AuthorizationRequests';
import { useTypedDispatch, useTypedSelector } from '../../Redux/store';
import { UpdateUser } from '../../Redux/Slicers/UserSlicer';
import { updateUserDataMutaion, RequestBuilder, updateUserAvatarMutation } from '../../Features/Queries';
import { ConnectToChat } from '../../Requests/Requests';
import { minPasswordLength, maxPasswordLength } from './SetPassword';
import { SetErrorHandler } from '../../SocketDispatcher';
import { setError, setState } from '../../Redux/Slicers/UserSlicer';
import GetElementInfDueToState from '../GetElementDueToState';
import RemoveUser from './RemoveUser';
import { updateAvatar } from '../../Redux/Slicers/UserSlicer';
import { ajaxUploadFile } from '../../Requests/Requests';
import Icon from '../Icon';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCameraAlt } from '@fortawesome/free-solid-svg-icons'
import { useTranslation } from 'react-i18next';
import SelectLanguage from './SelectLanguage';

export const minNickNameLength = 3;
export const maxNickNameLength = 75;


export default function UserSettings(props: { isVisible: boolean, setVisible: (v: boolean) => void }) {

    const { isVisible, setVisible } = props;

    const [emailToChange, setEmailToChange] = useState<null | string>(null);
    const [nickNameToChange, setNickNameToChange] = useState<null | string>(null);
    const [password, setPassword] = useState<string | null>(null);

    const [isDeleteVisible, setDeleteVisible] = useState(false)

    const [canSetEmail, setPosibilityChangeEmail] = useState(false);
    const [canSetNickName, setPosibilityChangeNickName] = useState(false);

    const dispatch = useTypedDispatch();
    const user = useTypedSelector(store => store.user.user);
    const state = useTypedSelector(store => store.user.status);
    const error = useTypedSelector(store => store.user.error);
    const { t } = useTranslation()
    const message = state === 'error' ? error : state === 'success' ? t('DefaultSuccessMessage') : undefined

    const passwordValidation = () => {
        if (!password)
            return false;

        return password.length >= minPasswordLength
            && password.length <= maxPasswordLength
    }

    const emailValidation = () => {
        let re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        if (!email)
            return false;

        return re.test(email)
    }

    const nickNameValidation = () => {

        if (!nickName)
            return false;

        return nickName.length >= minNickNameLength && nickName.length <= maxNickNameLength
    }

    const email = emailToChange !== null ? emailToChange : user?.email;
    const emailDisplayOrChange = canSetEmail ? <Form.Group controlId="validationCustom01" className='w-100'>
        <Form.Control
            isInvalid={!emailValidation()}
            value={email}
            onChange={(event) => setEmailToChange(event.target.value)}>
        </Form.Control>
        <Form.Control.Feedback type="invalid">
            {t('ValidationEmailError')}
        </Form.Control.Feedback>
    </Form.Group>
        : <div>{"Email: " + user?.email}</div>

    const nickName = nickNameToChange !== null ? nickNameToChange : user?.nickName;
    const nickNameDisplayOrChange = canSetNickName ? <Form.Group controlId="validationCustom01" className='w-100'>
        <Form.Control
            isInvalid={!nickNameValidation()}
            value={nickName}
            onChange={(event) => setNickNameToChange(event.target.value)}></Form.Control>
        <Form.Control.Feedback type="invalid">
            {t('ValidationNicknameError')}
        </Form.Control.Feedback>
    </Form.Group>
        : <div>{"Nickname: " + user?.nickName}</div>

    return <>
        <Modal
            show={isVisible}
            onHide={() => {
                setPassword(null)
                setEmailToChange(null)
                setNickNameToChange(null)
                dispatch(setState('idle'))
                setVisible(false)
            }}
            size='lg'
            centered>
            <Modal.Header closeButton className='h2'>{t('Settings')}</Modal.Header>
            <Modal.Body>
                <Row className='ms-1 mb-3'>
                    <Col>
                        <Icon name={user?.nickName ?? "nn"} color='yellow' onlyImage={true} src={user?.avatar} children={
                            <label htmlFor="selec-file-avatar" className='selec-file-avatar d-flex justify-content-center align-items-center h-100 w-100'>
                                <FontAwesomeIcon icon={faCameraAlt}></FontAwesomeIcon>
                            </label>
                        } />
                        <input type="file" accept="image/*" hidden id="selec-file-avatar" onChange={event => {
                            if (event.target.validity.valid && event.target.files && event.target.files[0]) {
                                const img = event.target.files[0]
                                try {
                                    ajaxUploadFile<{ updateUserAvatart: string }>(img, "file", updateUserAvatarMutation).subscribe({
                                        next: (data) => {
                                            dispatch(updateAvatar(data.updateUserAvatart))
                                        },
                                        error: () => {
                                            dispatch(setError(t('DefaultErrorMessage')))
                                        }
                                    })
                                } catch (error) {
                                    const strError = error as string
                                    if (strError)
                                        dispatch(setError(strError))
                                    else
                                        console.log(JSON.stringify(error));
                                }
                            }
                        }} />
                    </Col>
                    <Col className='d-flex justify-content-end'>
                        <SelectLanguage />
                    </Col>
                </Row>
                <Row>
                    <Col className='h5'>
                        <Form className='d-flex flex-column gap-3'>
                            {emailDisplayOrChange}
                            {nickNameDisplayOrChange}
                        </Form>
                    </Col>
                    <Col sm={5} className='d-flex flex-column gap-3 align-items-end'>
                        <Form.Check
                            onChange={(event) => {
                                setPosibilityChangeEmail(event.target.checked)
                            }}
                            checked={canSetEmail}
                            reverse
                            type="switch"
                            id="custom-switch"
                            label={t('UserSettings.changeEmailSwitch')}
                        />
                        <Form.Check
                            onChange={(event) => {
                                setPosibilityChangeNickName(event.target.checked)
                            }}
                            checked={canSetNickName}
                            reverse
                            type="switch"
                            id="custom-switch"
                            label={t('UserSettings.changeNicknameSwitch')}
                        />
                    </Col>
                </Row>
                <Row className='mt-3'>
                    <div className='w-100 d-flex justify-content-center'>
                        <Button variant='danger' className='w-100' onClick={() => {
                            setDeleteVisible(true)
                            setVisible(false)
                        }}>{t('UserSettings.removeAccount')}</Button>
                    </div>
                    <div className='mt-3 text-center'>
                        <GetElementInfDueToState state={state} message={message} />
                    </div>
                </Row>
            </Modal.Body>
            <Modal.Footer className='d-flex flex-row justify-content-between'>
                <div className='w-50 h5'>
                    <Form.Group controlId="validationCustom01" className='w-100'>
                        <Form.Control
                            isInvalid={!passwordValidation()}
                            type='password'
                            placeholder={t('UserSettings.enterPassPlaceholder')}
                            onChange={(event) => setPassword(event.target.value)}>
                        </Form.Control>
                        <Form.Control.Feedback type="invalid">
                            {t('ValidationPasswordError')}
                        </Form.Control.Feedback>
                    </Form.Group>
                </div>
                <div>
                    <Button type='submit' onClick={() => {
                        if (!emailValidation() || !nickNameValidation() || !passwordValidation()) {
                            return;
                        }
                        const data: UpdateUser = {
                            newPassword: password!,
                            oldPassword: password!,
                            email: email!,
                            nickName: nickName!
                        }

                        const connection = ConnectToChat()
                        connection.subscribe(sub => {

                            const request = RequestBuilder('start', {
                                query: updateUserDataMutaion, variables: {
                                    data
                                }
                            });

                            SetErrorHandler.SetHandler(request.id!, () => {
                                dispatch(setError("Uncorrect data, user with these email or nickname already exists or password is not right."))
                            })

                            sub.next(request, setState('pending'))
                        })
                    }} >{t('UserSettings.commitChanges')}</Button>
                </div>
            </Modal.Footer>
        </Modal >
        <RemoveUser isVisible={isDeleteVisible} setVisible={setDeleteVisible} />
    </>
}