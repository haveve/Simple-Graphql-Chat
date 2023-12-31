import React, { Dispatch, SetStateAction, useEffect, useState } from 'react';
import { Form, Button, Card, Modal, Row, Col, ProgressBar, InputGroup, ListGroup, Image } from "react-bootstrap";
import { ajaxFor2fDrop } from '../../Requests/AuthorizationRequests';
import { useTypedDispatch, useTypedSelector } from '../../Redux/store';
import { addUser } from '../../Redux/Slicers/UserSlicer';
import GetElementInfDueToState from '../GetElementDueToState';
import { sliceState as state } from '../../Redux/Slicers/AuthRegSlicer';
import { useTranslation } from 'react-i18next';

export default function Drop2factorAuht(props: { isVisibleDrop2fa: boolean, setVisibleDrop2fa: (v: boolean) => void }) {

    const { isVisibleDrop2fa, setVisibleDrop2fa } = props;
    const [code, setCode] = useState("");
    const [state, setState] = useState<state>({ state: 'idle' });

    const { t } = useTranslation();

    const dispatch = useTypedDispatch();
    const user = useTypedSelector(store => store.user.user);

    return <>
        <Modal
            show={isVisibleDrop2fa}
            onHide={() => {
                setVisibleDrop2fa(false)
                setState({ state: 'idle' })
            }}
            size='lg'
            centered>
            <Modal.Header closeButton className='h3'>{t('Drop2f')}</Modal.Header>
            <Modal.Body className='d-flex flex-row justify-content-between'>
                <div className='w-100 ms-3'>
                    <Form.Control className='w-100'
                        placeholder={('OneTimeCodePlaceholder')}
                        onChange={(event) => setCode(event.target.value)}>
                    </Form.Control>
                    <GetElementInfDueToState state={state.state} message={state.message} />
                    <Button variant='outline-warning'
                        className='w-100 mt-4'
                        onClick={() => {
                            setState({ state: 'pending' })
                            ajaxFor2fDrop(code).subscribe({
                                next: () => {
                                    setState({ message: t('DefaultSuccessMessage'), state: 'success' })
                                    dispatch(addUser({ ...(user!), key2Auth: null }));
                                },
                                error: (error) => {
                                    console.log(error)
                                    setState({ message: t('IncorrectOneTimeCode'), state: 'error' })
                                }
                            })
                        }}
                    >{t('Drop2f')}</Button>
                </div>
            </Modal.Body>
            <Modal.Footer>
            </Modal.Footer>
        </Modal>
    </>
}