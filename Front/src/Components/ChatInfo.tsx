import React, { useState, useEffect, useRef, JSX, forwardRef } from 'react';
import { Container, Row, Col, Form, Offcanvas } from 'react-bootstrap';
import { useTypedSelector } from '../Redux/store';
import { ConnectToChat } from '../Requests/Requests';
import { RequestBuilder } from '../Features/Queries';
import { queryParticipants } from '../Features/Queries';
import Icon from './Icon';
import RemoveFromChatById from './ChatOperation/RemoveFromChatById';
import { useImmer } from 'use-immer';
import { defaultState, ChatOptionType } from './ChatSelect';
import { ReduxParticipant } from '../Redux/Slicers/ChatSlicer';
import { setState } from '../Redux/Slicers/ChatSlicer';

export type ChatInfoOptionType = ChatOptionType & {
    userName?: string
}

export default function ChatInfo(props: { show: boolean, handleClose: () => void, children: JSX.Element }) {

    const chatPending = setState('pending');

    const { show, handleClose, children } = props;

    const [option, setOption] = useImmer<ChatInfoOptionType>(defaultState)
    const [search, setSearch] = useState<string | null>(null);

    const currentChatId = useTypedSelector(store => store.chat.currentChat?.id)
    const participants = useTypedSelector(store => store.chat.participants)

    const chatCreatorId = useTypedSelector(store => store.chat.currentChat?.creatorId)
    const userId = useTypedSelector(store => store.user.user?.id)

    const refToOpt = useRef<HTMLDivElement>(null)

    const HandleContextMenu = (data: ReduxParticipant, event: React.MouseEvent<HTMLDivElement>) => {
        event.preventDefault()
        if (refToOpt.current) {

            refToOpt.current.style.top = event.clientY + "px"
            refToOpt.current.style.left = event.clientX + "px"
        }

        if (chatCreatorId === userId && userId !== data.id) {
            setOption(el => {
                el.id = currentChatId
                el.show = true
                el.userName = data.nickName
            })
        }
    }

    const dropOptions = () => setOption(el => ({ ...el, show: false }))

    useEffect(() => {
        if (currentChatId && show) {
            const connection = ConnectToChat();
            const request = RequestBuilder('start', { query: queryParticipants, variables: { chatId: currentChatId, search } });
            connection.subscribe(sub => {
                sub.next(request, chatPending)
            })
            return () => {
                connection.subscribe(sub => {
                    sub.next(RequestBuilder('stop', {}, request.id!), chatPending)
                })
            }
        }
    }, [currentChatId, show, search]);


    const searchHandler: React.ChangeEventHandler<HTMLInputElement> = (event) => {
        setSearch(event.target.value)
    }

    return <Offcanvas show={show} onHide={handleClose} onClick={dropOptions} onMouseLeave={dropOptions} backdrop="static" className=''>
        <div className='mt-2 border-bottom border-secondary'>
            <div className='ps-1'>
                {children}
            </div>
            <div className='px-3'>
                <Form.Control onChange={searchHandler} className='round mb-2 border-dark'></Form.Control>
            </div>
        </div>
        <div className='pt-2 ps-2 h5 h-100 select-chat-scroll'>
            {participants.map(el => {
                return <div key={el.nickName} onContextMenu={(event) => HandleContextMenu(el, event)}>
                    <Icon color={el.color} name={el.nickName}>
                        <div className={`d-flex align-items-center small ${el.online ? 'text-success' : 'text-danger'} `}>{el.online ? "online" : "offline"}</div>
                    </Icon>
                </div>

            })}
        </div>
        <Options ref={refToOpt} option={option} />
    </Offcanvas>
}

export const Options = forwardRef<HTMLDivElement, { option: ChatInfoOptionType }>((props, ref) => {
    const { option } = props;
    const [showRemoveUser, setShowRemoveUser] = useState(false)

    const removeUserIco = <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" className="bi mb-1 me-1 bi-person-dash" viewBox="0 0 16 16">
        <path d="M12.5 16a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7ZM11 12h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1 0-1Zm0-7a3 3 0 1 1-6 0 3 3 0 0 1 6 0ZM8 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z" />
        <path d="M8.256 14a4.474 4.474 0 0 1-.229-1.004H3c.001-.246.154-.986.832-1.664C4.484 10.68 5.711 10 8 10c.26 0 .507.009.74.025.226-.341.496-.65.804-.918C9.077 9.038 8.564 9 8 9c-5 0-6 3-6 4s1 1 1 1h5.256Z" />
    </svg>

    return <div className={`${option.show ? '' : 'd-none'} chat-info-option d-flex flex-column py-3 position-absolute`} ref={ref}>
        <div className='text-danger' onClick={() => setShowRemoveUser(true)}>{removeUserIco}Remove user</div>
        <RemoveFromChatById userName={option.userName!} chatId={option.id!} show={showRemoveUser} setShow={setShowRemoveUser} />
    </div>
})