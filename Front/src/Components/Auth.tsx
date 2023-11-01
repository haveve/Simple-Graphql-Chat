import React, { useState } from 'react'
import { Container, Row, Col, Form, Button } from 'react-bootstrap';
import { logo, title } from '../Features/Constants';
export default function Auth() {

    const [nickNameOrEmail, setNickNameOrEmail] = useState('')
    const [password, setPassword] = useState('')
    const [validated,setValidated] = useState<boolean|undefined>()

    const nickNameOrEmailOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setNickNameOrEmail(event.target.value)
    }

    const passwordOnChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setPassword(event.target.value)
    }

    const handleSubmit:React.FormEventHandler<HTMLFormElement> = (event) => {
        event.preventDefault()
        const form = event.currentTarget;
        if (form.checkValidity() === true) {

            return;
        }    
        setValidated(true);
      };

    return <Row className='justify-content-center h-75 align-items-center p-0 m-0'>
        <Col sm='3' className='bg-secondary p-3 rounded'>
            <Form className='d-flex flex-column gap-4' noValidate onSubmit={handleSubmit} validated = {validated}>
                <Form.Group controlId="validationCustom01">
                    <Form.Control size='lg' required value={nickNameOrEmail} onChange={nickNameOrEmailOnChange} className='mt-3' placeholder='Nick name or email'></Form.Control>
                    <Form.Control.Feedback type="invalid">
                        Please provide a valid nickname or email
                    </Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="validationCustom02">
                    <Form.Control size='lg' required value={password} onChange={passwordOnChange} placeholder='Password' type="password"></Form.Control>
                    <Form.Control.Feedback  type="invalid">
                        Please provide a password
                    </Form.Control.Feedback>
                </Form.Group>
                <Button type='submit' variant='outline-warning' size='lg'>
                    Log in
                </Button>
            </Form>
        </Col>
    </Row>

}
