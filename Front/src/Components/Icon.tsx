import React, { useState, useEffect, useRef, JSX } from 'react';
import { Container, Row, Col, Form, Offcanvas } from 'react-bootstrap';
import { GetAbbreviationFromPhrase } from '../Features/Functions';

export default function Icon(props:{color:string,name:string,children?:JSX.Element|null,withChatInfo?:boolean,handleChatInfo?:()=>void}){

    const {color,name,children,withChatInfo,handleChatInfo} = props;

    return <div className='d-flex' role = {withChatInfo?"button":"img"} onClick={handleChatInfo}>
    <div className='chat-title-icon-size h5 ms-2' style={{
        backgroundColor: color
    }}>
        {GetAbbreviationFromPhrase(name)}
    </div>
    <div className='h5 pt-1 ms-3 d-flex flex-column justify-content-center align-items-start'>
        {name}
        {children}
    </div>
</div>
}