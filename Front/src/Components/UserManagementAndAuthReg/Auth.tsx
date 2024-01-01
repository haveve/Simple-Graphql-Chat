import React, { useState,useEffect } from 'react'
import { Container, Row, Col, Form,Nav, Button } from 'react-bootstrap';
import { ajaxForLogin } from '../../Requests/AuthorizationRequests';
import { useNavigate } from "react-router-dom";
import { setCookie } from '../../Features/Functions';
import GetElementInfDueToState from './../GetElementDueToState';
import { setState } from '../../Redux/Slicers/AuthRegSlicer';
import { useTypedDispatch,useTypedSelector } from '../../Redux/store';

export default function Auth() {
    const [nickNameOrEmail, setNickNameOrEmail] = useState('')
    const [password, setPassword] = useState('')
    const [validated, setValidated] = useState<boolean | undefined>()

    const {message,state} = useTypedSelector(store => store.auth_reg)

    const dispatch = useTypedDispatch()

    const navigate = useNavigate()

    const nickNameOrEmailOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setNickNameOrEmail(event.target.value)
    }

    const passwordOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setPassword(event.target.value)
    }

    useEffect(() => {
        const queryParameters = new URLSearchParams(window.location.search)
        const token = queryParameters.get("token")
        const expiredAt = queryParameters.get("expiredAt")
        const issuedAt = queryParameters.get("issuedAt")
        if (token) {
          setCookie({
            name: "refresh_token",
            value: JSON.stringify({ token, issuedAt, expiredAt }),
            expires_second: new Date(expiredAt!).getTime() / 1000,
            path: "/"
          });
          navigate("/main");
        }
      }, [])

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (event) => {
        event.preventDefault()
        const form = event.currentTarget;
        if (form.checkValidity()) {
            dispatch(setState({state:'pending'}))
            ajaxForLogin({
                login: {
                    password,
                    nickNameOrEmail
                }
            }).subscribe({
                next:(data)=>{
                    setCookie({ name: "refresh_sent", value: "false" })
                    dispatch(setState({state:'success',message:'you was signed in'}))
                    navigate(data)
                    
                },
                error:()=>{
                    dispatch(setState({state:'error',message:'uncorrect login or password'}))
                }
            })
            return;
        }
        setValidated(true);
    };

    const navigateToResetPassword = ()=>{
        navigate('/reset-password')
    }

    return <Form className='d-flex flex-column gap-4' noValidate onSubmit={handleSubmit} validated={validated}>
                <h1>Sign In</h1>
                <Form.Group controlId="validationCustom01" className='w-100 m-0'>
                    <Form.Control size='lg' required value={nickNameOrEmail} onChange={nickNameOrEmailOnChange} className='m-0 mt-3 w-100' placeholder='Nick name or email' type='text'></Form.Control>
                    <Form.Control.Feedback type="invalid">
                        Please provide a valid nickname or email
                    </Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="validationCustom02" className='w-100 m-0'>
                    <Form.Control size='lg' required value={password} onChange={passwordOnChange} placeholder='Password' className='m-0 w-100' type="password"></Form.Control>
                    <Form.Control.Feedback type="invalid">
                        Please provide a password
                    </Form.Control.Feedback>
                    <Nav className="justify-content-center m-0" >
                    <Nav.Item className='m-0'>
                        <Nav.Link className='m-0' onClick={navigateToResetPassword}>
                            <h6>Forgot passsowrd? Click </h6></Nav.Link>
                    </Nav.Item>
                </Nav>
                </Form.Group>
                <Button type='submit' variant='outline-warning' size='lg'>
                    Log in
                </Button>
                <GetElementInfDueToState message = {message} state = {state}/>
            </Form>

}
