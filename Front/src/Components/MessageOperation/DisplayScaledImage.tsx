import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Container, Row, Col, Form, Modal, Button } from 'react-bootstrap';
import store, { useTypedSelector } from '../../Redux/store';
import { useDispatch } from 'react-redux';
import { dropImageToScaledShow } from '../../Redux/Slicers/ChatSlicer';

export default function DisplayScaledImage() {
    const img = useTypedSelector(store => store.chat.imageToScaledShow)
    const dispatch = useDispatch();

    return <Modal size='lg' centered onHide={() => {
        dispatch(dropImageToScaledShow())
    }} show={img !== undefined}>
        < Modal.Body className='d-flex justify-content-center m-0 p-0'>
            <img className='w-100' src={img ? img : ""} alt="" />
        </Modal.Body >
    </Modal >
}
