import React, { useState } from 'react'
import { Nav, Form, Button } from 'react-bootstrap';
import { RequestPasswordReset } from '../../Requests/AuthorizationRequests';
import { useTypedDispatch, useTypedSelector } from '../../Redux/store';
import GetElementInfDueToState from '../GetElementDueToState';
import { setState } from '../../Redux/Slicers/AuthRegSlicer';
import { useNavigate } from 'react-router';
import '../../Styles/LoginLogout.css'

export default function ResetPassword() {
    const [emailOrNickname, setEmailOrNickname] = useState('')
    const [validated, setValidated] = useState<boolean | undefined>()

    const dispatch = useTypedDispatch();
    const navigate = useNavigate();

    const { message, state } = useTypedSelector(store => store.auth_reg)

    const emailOrNicknameOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setEmailOrNickname(event.target.value)
    }

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (event) => {
        event.preventDefault()
        const form = event.currentTarget;
        if (form.checkValidity()) {
            dispatch(setState({ state: 'pending' }))
            RequestPasswordReset(emailOrNickname).subscribe({
                next: () => {
                    dispatch(setState({ message: 'check email', state: 'success' }))
                },
                error: () => {
                    dispatch(setState({ message: 'invalid data', state: 'error' }))
                }
            });
            return;
        }
        setValidated(true);
    };

    const navigateToLogin = () => {
        dispatch(setState({ state: 'idle' }))
        navigate("/")
    }

    return <div className='h-100 bg-container w-100 d-flex justify-content-center align-items-center'>
        <Form noValidate onSubmit={handleSubmit} validated={validated} className='container h-50 w-25 justify-content-center d-flex flex-column gap-1'>
            <h1 className='text-center pb-4'>Reset password</h1>
            <Form.Group controlId="validationCustom01" className='w-100 d-flex justify-content-center'>
                <Form.Control size='lg' required value={emailOrNickname} type='text' onChange={emailOrNicknameOnChange} className='m-0 mb-3 w-75' placeholder='Email or nickname'></Form.Control>
                <Form.Control.Feedback type="invalid">
                    Please provide a email or nickname
                </Form.Control.Feedback>
            </Form.Group>
            <div className='d-flex justify-content-center align-items-center'>
                <Button type="submit" variant='outline-warning' size='lg'>
                    Reset password
                </Button>
            </div>
            <Nav className="justify-content-center m-0 " >
                <Nav.Item className='m-0'>
                    <Nav.Link className='m-0' onClick={navigateToLogin}>
                        <h6>Sign In</h6></Nav.Link>
                </Nav.Item>
            </Nav>
            <div className='text-center'>
                <GetElementInfDueToState message={message} state={state} />
            </div>
        </Form>
    </div>
}