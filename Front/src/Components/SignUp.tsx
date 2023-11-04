import React, { useState } from 'react'
import { Container, Row, Col, Form, Button } from 'react-bootstrap';
import { ajaxForRegistration } from '../Requests/AuthorizationRequests';
export default function SignUp() {

    const [nickName, setNickName] = useState('')
    const [email, setEmail] = useState('')
    const [validated, setValidated] = useState<boolean | undefined>()

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
            ajaxForRegistration({
                registration:{
                    email,
                    nickName
                }
            }).subscribe();
            return;
        }
        setValidated(true);
    };

    return <Row className='justify-content-center h-75 align-items-center p-0 m-0'>
        <Col sm='3' className='bg-secondary p-3 rounded'>
            <Form noValidate onSubmit={handleSubmit} validated={validated} className='d-flex flex-column gap-4'>
                <Form.Group controlId="validationCustom01">
                    <Form.Control size='lg' required value={nickName} onChange={nickNameOnChange} className='mt-3' placeholder='Nickname'></Form.Control>
                    <Form.Control.Feedback  type="invalid">
                        Please provide a valid nickname
                    </Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="validationCustom01">
                    <Form.Control size='lg' required value={email} type = 'email'  onChange={emailOnChange} placeholder='Email'></Form.Control>
                    <Form.Control.Feedback type="invalid">
                        Please provide a valid email
                    </Form.Control.Feedback>
                </Form.Group>
                <Button type="submit" variant='outline-warning' size='lg'>
                    Sign up
                </Button>
            </Form>
        </Col>
    </Row>

}