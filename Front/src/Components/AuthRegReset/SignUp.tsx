import React, { useState } from 'react'
import { Container, Row, Col, Form, Button } from 'react-bootstrap';
import { ajaxForRegistration } from '../../Requests/AuthorizationRequests';
import { useTypedDispatch, useTypedSelector } from '../../Redux/store';
import GetElementInfDueToState from '../GetElementDueToState';
import { setState } from '../../Redux/Slicers/AuthRegSlicer';

export default function SignUp() {

    const [nickName, setNickName] = useState('')
    const [email, setEmail] = useState('')
    const [validated, setValidated] = useState<boolean | undefined>()

    const dispath = useTypedDispatch();
    const { message, state } = useTypedSelector(store => store.auth_reg)

    const nickNameOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setNickName(event.target.value)
    }

    const emailOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setEmail(event.target.value)
    }

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (event) => {
        event.preventDefault()
        const form = event.currentTarget;
        if (form.checkValidity()) {
            dispath(setState({ state: 'pending' }))
            ajaxForRegistration({
                registration: {
                    email,
                    nickName
                }
            }).subscribe({
                next: () => {
                    dispath(setState({ message: 'check email', state: 'success' }))
                },
                error: () => {
                    dispath(setState({ message: 'user with that data already exists', state: 'error' }))
                }
            });
            return;
        }
        setValidated(true);
    };

    return <Form noValidate onSubmit={handleSubmit} validated={validated} className='d-flex flex-column gap-4'>
        <h1>Sign Up</h1>
        <Form.Group controlId="validationCustom01" className='w-100'>
            <Form.Control size='lg' required value={nickName} onChange={nickNameOnChange} className='m-0 mt-3 w-100' placeholder='Nickname'></Form.Control>
            <Form.Control.Feedback type="invalid">
                Please provide a valid nickname
            </Form.Control.Feedback>
        </Form.Group>
        <Form.Group controlId="validationCustom01" className='w-100'>
            <Form.Control size='lg' required value={email} type='email' onChange={emailOnChange} className='m-0 mb-3 w-100' placeholder='Email'></Form.Control>
            <Form.Control.Feedback type="invalid">
                Please provide a valid email
            </Form.Control.Feedback>
        </Form.Group>
        <Button type="submit" variant='outline-warning' size='lg'>
            Sign up
        </Button>
        <GetElementInfDueToState message={message} state={state} />
    </Form>

}