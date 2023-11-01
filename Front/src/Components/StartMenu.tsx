import React from 'react'
import { Container, Row, Col, Button } from 'react-bootstrap';
import { logo, title } from '../Features/Constants';
import { Outlet,useNavigate } from "react-router-dom";

export default function StartMenu() {

    const navigate = useNavigate()

    return <Container fluid='fluid' className='h-100'>
        <Row className='p-3 bg-secondary mb-3 flex-row'>
            <Col className='d-flex flex-row gap-5 mb-2 h3 text-white'>
                <span>{logo}</span>
                <span>{title}</span>
            </Col>
            <Col className='justify-content-end gap-4 d-flex'>
                <Button variant='outline-warning' size='lg' onClick={()=>navigate('/')}>Log in</Button>
                <Button variant='outline-warning' size='lg' onClick={()=>navigate('/sign-in')}>Sign up</Button>
            </Col>
        </Row>
        <Outlet />
    </Container>
}