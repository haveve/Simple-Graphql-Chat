import React, { useState, useEffect, useRef } from 'react';
import { Container, Row, Col, Form } from 'react-bootstrap';
import { GetAbbreviationFromPhrase } from '../Features/Functions';
import { useTypedSelector } from '../Redux/store';
import { ReduxChat, ReduxCurrentChat } from '../Redux/Slicers/ChatSlicer';
import ChatInfo from './ChatInfo';
import Icon from './Icon';

export default function ChatHeader(props: { currentChat: ReduxCurrentChat, withChatInfo?: boolean, onlyIco?: boolean, withoutParticipants?: boolean }) {

    const { currentChat, onlyIco, withoutParticipants, withChatInfo } = props
    const [showChatInfo, setShowChatInfo] = useState(false);
    const participantText = 'participant'

    const handleChatInfo = () => {
        setShowChatInfo(inf => !inf)
    }

    const participants = withoutParticipants ? null : <span className='participants'>
        {currentChat!.chatMembersCount === 0 ?
            currentChat!.chatMembersCount + 1 + ' ' + participantText :
            currentChat!.chatMembersCount + 1 + ' ' + participantText + 's'}
    </span>


    const ico = <div className='d-flex' role = {withChatInfo?"button":"img"} onClick={handleChatInfo}>
        <div className='chat-title-icon-size h5 ms-2' style={{
            backgroundColor: currentChat!.color
        }}>
            {GetAbbreviationFromPhrase(currentChat!.name)}
        </div>
        <div className='h5 pt-1 ms-3 d-flex flex-column justify-content-center align-items-start'>
            {currentChat!.name}
            {participants}
        </div>
    </div>


    const chatInfo = withChatInfo ? <ChatInfo children={ico} show={showChatInfo} handleClose={handleChatInfo} /> : null

    const returned = onlyIco ? ico : <div className='chat-list p-2 d-flex justify-content-between'>
        <Icon color={currentChat!.color} name={currentChat!.name} withChatInfo = {withChatInfo} handleChatInfo={handleChatInfo}>
            {participants}
        </Icon>
        <div className='d-flex align-items-center'>
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" className="more-chat-info bi bi-three-dots-vertical" viewBox="0 0 16 16">
                <path d="M9.5 13a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0z" />
            </svg>
        </div>
    </div>

    return <>
        {returned}
        {chatInfo}
    </>
}