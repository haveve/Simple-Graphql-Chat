import React, { useState } from 'react'
import { Container, Row, Col, Form, Nav, Button } from 'react-bootstrap';
import { ajaxSetPasswordByCode } from '../../Requests/AuthorizationRequests';
import '../../Styles/LoginLogout.css'
import GetElementInfDueToState from '../GetElementDueToState';
import { setState } from '../../Redux/Slicers/AuthRegSlicer';
import { useTypedDispatch, useTypedSelector } from '../../Redux/store';
import { useNavigate } from 'react-router';

export default function SetPassword() {

    const maxPasswordLength = 16;
    const minPasswordLength = 8

    const navigate = useNavigate();

    const { message, state } = useTypedSelector(store => store.auth_reg)
    const dispatch = useTypedDispatch()

    const [password, setPassword] = useState('')
    const [repeatedPassword, setRepeatedPassword] = useState('')

    const passwordValidation = () => {
        return password.length >= minPasswordLength
            && password.length <= maxPasswordLength
    }

    const repeatedPasswordValidation = () => {
        return repeatedPassword == password
    }

    const passwordOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setPassword(event.target.value)
    }

    const repeatedPasswordOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setRepeatedPassword(event.target.value)
    }

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (event) => {
        event.preventDefault()
        if (passwordValidation() && repeatedPasswordValidation()) {
            dispatch(setState({ state: 'pending' }))
            const queryParameters = new URLSearchParams(window.location.search)
            const code = queryParameters.get("code") ?? ""
            const email = queryParameters.get("email") ?? ""
            ajaxSetPasswordByCode({
                code, password, email
            }).subscribe({
                next: () => {
                    dispatch(setState({ state: 'success', message: 'password was set' }))
                },
                error: () => {
                    dispatch(setState({ state: 'error', message: 'incorrect data' }))
                }
            });
            return;
        }
    };

    const navigateToLogin = ()=>{
        dispatch(setState({state:'idle'}))
        navigate("/")
    }

    return <div className='bg-container justify-content-center h-100 w-100 d-flex align-items-center p-0 m-0'>
        <Form noValidate onSubmit={handleSubmit} className='d-flex flex-column p-5 w-25 justify-content-center container gap-4'>
            <h1 className='text-center'>Set password</h1>
            <Form.Group controlId="validationCustom01">
                <Form.Control size='lg' value={password} type='password' isInvalid={password?!passwordValidation():false} onChange={passwordOnChange} className='mt-3' placeholder='Password'></Form.Control>
                <Form.Control.Feedback type="invalid">
                    Password must be betwee {minPasswordLength} and {maxPasswordLength}
                </Form.Control.Feedback>
            </Form.Group>
            <Form.Group controlId="validationCustom01">
                <Form.Control size='lg' value={repeatedPassword} isInvalid={!repeatedPasswordValidation()} type='password' onChange={repeatedPasswordOnChange} placeholder='Repeated password'></Form.Control>
                <Form.Control.Feedback type="invalid">
                    Repeated password must be equealed to password
                </Form.Control.Feedback>
            </Form.Group>
            <Button type="submit" variant='outline-warning' size='lg'  className='m-0'>
                Set password
            </Button>
            <div className='text-center m-0'>
            <Nav className="justify-content-center m-0" >
                    <Nav.Item className='m-0'>
                        <Nav.Link className='m-0' onClick={navigateToLogin}>
                            <h6>Sign In</h6></Nav.Link>
                    </Nav.Item>
                </Nav>
                <GetElementInfDueToState message={message} state={state} />
            </div>
        </Form>
    </div>

}