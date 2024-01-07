import React, { useState, useEffect, useRef, JSX } from 'react';
import { Container, Row, Col, Form, OverlayTrigger, Tooltip } from 'react-bootstrap';
import { GetAbbreviationFromPhrase, randomIntFromInterval } from '../Features/Functions';
import { ReduxChat, ReduxCurrentChat } from '../Redux/Slicers/ChatSlicer';
import ChatInfo from './ChatInfo';
import Icon from './Icon';
import { SmilesList, SmilesWithComments, SmileListType } from '../Features/Constants';
import { useTranslation } from 'react-i18next';

export default function ChatHeader(props: { currentChat: ReduxCurrentChat, onSmileClick?: () => void, withChatInfo?: boolean, onlyIco?: boolean, withoutParticipants?: boolean }) {

    const { currentChat, onSmileClick, onlyIco, withoutParticipants, withChatInfo } = props
    const [showChatInfo, setShowChatInfo] = useState(false);
    const [randomImg, setRandomImg] = useState<SmileListType | null>(null)

    const { t } = useTranslation();

    const participantText = t('Participant').toLocaleLowerCase()

    const handleChatInfo = () => {
        setShowChatInfo(inf => !inf)
    }

    useEffect(() => {
        setRandomImg(SmilesWithComments[randomIntFromInterval(0, SmilesList.length - 1)])
    }, [SmilesList.length, currentChat.id])

    const participants = withoutParticipants ? null : <span className='participants'>
        {currentChat!.chatMembersCount + 1 + ' ' + participantText}
    </span>

    const ico = <div className='d-flex' role={withChatInfo ? "button" : "img"} onClick={handleChatInfo}>
        <Icon color={currentChat!.color} src={currentChat.avatar} name={currentChat!.name}>
            {participants}
        </Icon>
    </div>

    const chatInfo = withChatInfo ? <ChatInfo children={ico} show={showChatInfo} handleClose={handleChatInfo} /> : null

    const SmileTip = (props: React.ComponentProps<any>) => {
        return <Tooltip id="button-tooltip" {...props}>
            <span className='smile-text-color'>
                {t(randomImg?.message ?? "")}
            </span>
        </Tooltip>
    }


    const returned = onlyIco ? ico : <><div className='chat-list p-2 d-flex justify-content-between'>
        <Icon color={currentChat!.color} src={currentChat.avatar} name={currentChat!.name} withChatInfo={withChatInfo} handleChatInfo={handleChatInfo}>
            {participants}
        </Icon>
        <OverlayTrigger
            placement="bottom"
            delay={{ show: 250, hide: 400 }}
            overlay={SmileTip}
        >
            <div className='chat-header-emoji' onClick={onSmileClick}>
                {randomImg?.picture}
            </div>
        </OverlayTrigger>
    </div></>

    return <>
        {returned}
        {chatInfo}
    </>
}
