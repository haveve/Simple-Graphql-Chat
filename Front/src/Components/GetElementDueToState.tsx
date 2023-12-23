import React, { useState, useEffect, useRef, useMemo, memo, forwardRef } from 'react';
import { Status } from '../Redux/Slicers/ChatSlicer';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faBomb, faCheck } from '@fortawesome/free-solid-svg-icons'
import Spinner from 'react-bootstrap/Spinner';

export default function GetElementInfDueToState(props: { message?: string, state: Status }) {

    const { message, state } = props

    switch (state) {
        case 'error':
            return <div className='text-danger'>{message}
                <FontAwesomeIcon className='ms-2' icon={faBomb}></FontAwesomeIcon>
            </div>
        case 'success':
            return <div className='text-success'>{message}
                <FontAwesomeIcon className='ms-2' icon={faCheck}></FontAwesomeIcon>
            </div>
        case 'pending':
            return <div className='text-secondary'>sending
            <Spinner className='ms-2' size='sm' animation="border" />
        </div>
    }
    return <div></div>
}