import React, { useState, forwardRef } from 'react'
import { Container, Row, Col, Form, Button, Dropdown } from 'react-bootstrap';
import { ajaxForLogout,GetRefresh,TokenErrorHandler } from '../../Requests/AuthorizationRequests';
export default function LogOut() {


    const logOutHandler = ()=>{
        ajaxForLogout(GetRefresh()).subscribe()
        TokenErrorHandler()
    }

    return <Dropdown>
        <Dropdown.Toggle as={CustomToggle} id="dropdown-basic">
        </Dropdown.Toggle>
        <Dropdown.Menu>
            <Dropdown.Item onClick={logOutHandler}>Log out</Dropdown.Item>
        </Dropdown.Menu>
    </Dropdown>
}

const CustomToggle = React.forwardRef<any, { children: any, onClick: any }>(({ children, onClick }, ref) => (
    <a
        href=""
        ref={ref}
        onClick={(e) => {
            e.preventDefault();
            onClick(e);
        }}
    >
        <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" fill="gray" className="custom-svg bi bi-three-dots-vertical" viewBox="0 0 16 16">
            <path d="M9.5 13a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0m0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0m0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0" />
        </svg>
    </a>
));
