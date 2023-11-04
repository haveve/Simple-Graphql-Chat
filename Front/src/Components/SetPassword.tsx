import React, { useState } from 'react'
import { Container, Row, Col, Form, Button } from 'react-bootstrap';
import { ajaxSetPasswordByCode } from '../Requests/AuthorizationRequests';
export default function SetPassword() {

    const maxPasswordLength = 16;
    const minPasswordLength = 8

    const [password, setPassword] = useState('')
    const [repeatedPassword, setRepeatedPassword] = useState('')
    const [validated, setValidated] = useState<boolean | undefined>()

    const passwordValidation = ()=>{
        return  minPasswordLength <= password.length 
                && password.length <= maxPasswordLength
    }

    const repeatedPasswordValidation = () =>{
        return  minPasswordLength <= repeatedPassword.length 
                && repeatedPassword.length <= maxPasswordLength
                && repeatedPassword == password
    }

    const passwordOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setPassword(event.target.value)
    }

    const repeatedPasswordOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setRepeatedPassword(event.target.value)
    }

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (event) => {
        event.preventDefault()
        const form = event.currentTarget;
        if (form.checkValidity()) {
            const queryParameters = new URLSearchParams(window.location.search)
            const code = queryParameters.get("code")??""
            const email = queryParameters.get("email")??""
            ajaxSetPasswordByCode({
                code,password,email
            }).subscribe();
            return;
        }
        setValidated(true);
    };

    return <Row className='justify-content-center h-75 align-items-center p-0 m-0'>
        <Col sm='3' className='bg-secondary p-3 rounded'>
            <Form noValidate onSubmit={handleSubmit} validated={validated} className='d-flex flex-column gap-4'>
                <Form.Group controlId="validationCustom01">
                    <Form.Control size='lg' required value={password} type = 'password' isInvalid = {!passwordValidation()} onChange={passwordOnChange} className='mt-3' placeholder='Password'></Form.Control>
                    <Form.Control.Feedback  type="invalid">
                        Password must be betwee {minPasswordLength} and {maxPasswordLength}
                    </Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="validationCustom01">
                    <Form.Control size='lg' required value={repeatedPassword} isInvalid = {!repeatedPasswordValidation()} type = 'password'  onChange={repeatedPasswordOnChange} placeholder='Repeated password'></Form.Control>
                    <Form.Control.Feedback type="invalid">
                        Repeated password must be equealed to password
                    </Form.Control.Feedback>
                </Form.Group>
                <Button type="submit" variant='outline-warning' size='lg'>
                    Set password
                </Button>
            </Form>
        </Col>
    </Row>

}