import React, { useEffect, useState } from 'react';
import { Container, Row, Col, Form,Button } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import '../Styles/Custom.scss'

export default function NotFound(){

    const navigate = useNavigate();

    const onClickNvigate = ()=>{
        navigate("/")
    }

    const waringSign = <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="bi warning-sign bi-exclamation-diamond" viewBox="0 0 16 16">
    <path d="M6.95.435c.58-.58 1.52-.58 2.1 0l6.515 6.516c.58.58.58 1.519 0 2.098L9.05 15.565c-.58.58-1.519.58-2.098 0L.435 9.05a1.482 1.482 0 0 1 0-2.098L6.95.435zm1.4.7a.495.495 0 0 0-.7 0L1.134 7.65a.495.495 0 0 0 0 .7l6.516 6.516a.495.495 0 0 0 .7 0l6.516-6.516a.495.495 0 0 0 0-.7L8.35 1.134z"/>
    <path d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z"/>
  </svg>


    return <Container fluid className='p-0 m-0 bg-black h-100'>
        <Row className='p-0 m-0 d-flex justify-content-center flex-column align-items-center h-100'>
            <p className='text-center text-warning'>
                {waringSign}
            </p>
            <p className='text-center h1 text-warning'>
                Ooops...
            </p>
            <p className='text-center h5 text-warning'>
                Page was not found 404
            </p>
            <p className = 'text-center h6 text-warning'>
                You try to visit page that either does not exists or you have not enough rights. 
            </p>
            <Button onClick={onClickNvigate} className='w-25 mt-3' variant='outline-warning'>
                {"Back to main"}
                </Button>
        </Row>
    </Container>
}