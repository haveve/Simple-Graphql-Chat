import React, { useState, useEffect, useRef, JSX } from 'react';
import { Container, Row, Col, Form, OverlayTrigger, Tooltip } from 'react-bootstrap';
import { GetAbbreviationFromPhrase, randomIntFromInterval } from '../Features/Functions';
import { useTypedSelector } from '../Redux/store';
import { ReduxChat, ReduxCurrentChat } from '../Redux/Slicers/ChatSlicer';
import ChatInfo from './ChatInfo';
import Icon from './Icon';
import { SmilesList, SmilesWithComments, SmileListType } from '../Features/Constants';

export default function ChatHeader(props: { currentChat: ReduxCurrentChat, withChatInfo?: boolean, onlyIco?: boolean, withoutParticipants?: boolean }) {

    const { currentChat, onlyIco, withoutParticipants, withChatInfo } = props
    const [showChatInfo, setShowChatInfo] = useState(false);
    const [randomImg, setRandomImg] = useState<SmileListType | null>(null)
    const participantText = 'participant'

    const handleChatInfo = () => {
        setShowChatInfo(inf => !inf)
    }

    useEffect(() => {
        setRandomImg(SmilesWithComments[randomIntFromInterval(0, SmilesList.length - 1)])
    }, [SmilesList.length, currentChat.id])

    const participants = withoutParticipants ? null : <span className='participants'>
        {currentChat!.chatMembersCount === 0 ?
            currentChat!.chatMembersCount + 1 + ' ' + participantText :
            currentChat!.chatMembersCount + 1 + ' ' + participantText + 's'}
    </span>


    const ico = <div className='d-flex' role={withChatInfo ? "button" : "img"} onClick={handleChatInfo}>
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

    const SmileTip = (props:React.ComponentProps<any>) => {
        return <Tooltip  id="button-tooltip"{...props}>
            {randomImg?.message}
        </Tooltip>
    }


    const returned = onlyIco ? ico : <><div className='chat-list p-2 d-flex justify-content-between'>
        <Icon color={currentChat!.color} name={currentChat!.name} withChatInfo={withChatInfo} handleChatInfo={handleChatInfo}>
            {participants}
        </Icon>
        <OverlayTrigger
            placement="bottom"
            delay={{ show: 250, hide: 400 }}
            overlay={SmileTip}
        >
            <div className='chat-header-emoji'>
                {randomImg?.picture}
            </div>
        </OverlayTrigger>
    </div></>

    return <>
        {returned}
        {chatInfo}
    </>
}