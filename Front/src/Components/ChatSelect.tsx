import React, { useState, useEffect, useRef, useMemo, memo, forwardRef } from 'react';
import { GetAbbreviationFromPhrase, GetDisplayedName } from '../Features/Functions';
import { useTypedSelector, useTypedDispatch } from '../Redux/store';
import UpdateChat from './ChatOperation/UpdateChat';
import RemoveChat from './ChatOperation/RemoveChat';
import { Updater, useImmer } from "use-immer";
import AddUserToChat from './ChatOperation/AddUser';
import LeaveFromChat from './ChatOperation/LeaveFromChat'
import RemoveFromChat from './ChatOperation/RemoveFromChat';
import { ConnectToChat } from '../Requests/Requests';
import { RequestBuilder } from '../Features/Queries';
import { queryFullChatInfo } from '../Features/Queries';
import { selectChatIds } from '../Redux/reselect';
import { setState } from '../Redux/Slicers/ChatSlicer';

export type ChatOptionType = {
    show: boolean,
    id?: number
}

export const defaultState = { show: false }

export default function ChatSelect() {

    const userChatsId = useTypedSelector(selectChatIds)
    const [option, setOption] = useImmer<ChatOptionType>(defaultState)
    const refToOpt = useRef<HTMLDivElement>(null)

    const dropOptions = () => setOption(el => ({ ...el, show: false }))

    return <div onMouseLeave={dropOptions} onClick={dropOptions} onContextMenu={(event) => {
        event.preventDefault()
    }} >
        <Options ref={refToOpt} option={option} />
        {userChatsId.map((id) =>
            <ChatSel key={id} id={id} setOption={setOption} refToOpt={refToOpt} />
        )}
    </div>
}

export const ChatSel = memo((props: { id: number, setOption: Updater<ChatOptionType>, refToOpt: React.RefObject<HTMLDivElement> }) => {
    const chatPending = setState('pending');
    
    const { id, setOption, refToOpt } = props;
    const el = useTypedSelector(store => store.chat.chats.find(el => el.id === id)!)
    const currentChat = useTypedSelector(store => store.chat.currentChat)

    const HandleChatClick = (event: React.MouseEvent<HTMLDivElement>) => {
        event.preventDefault()
        const connection = ConnectToChat()
        connection.subscribe(sub => {
            sub.next(RequestBuilder('start', { query: queryFullChatInfo, variables: { chatId: el.id } }),chatPending)
        })
    }

    const HandleContextMenu = (event: React.MouseEvent<HTMLDivElement>) => {
        event.preventDefault()
        if (refToOpt.current) {
            refToOpt.current.style.top = event.clientY + "px"
            refToOpt.current.style.left = event.clientX + "px"
        }

        setOption(el => {
            el.id = id
            el.show = true
        })
    }


    return <div onContextMenu={HandleContextMenu} onClick={currentChat?.id === el.id ? undefined : HandleChatClick}
        className={`${currentChat?.id === el.id ? 'selected' : null}  chat chat-hover p-2 h5`}>
        <div className='chat-icon-size' style={{
            backgroundColor: el.color
        }}>{GetAbbreviationFromPhrase(el.name)}</div>
        <span className={`ps-2`}>{GetDisplayedName(el.name)}</span>
    </div>
})


export const Options = forwardRef<HTMLDivElement, {option: ChatOptionType }>((props, ref) => {
    const { option} = props;
    const [showUpdate, setShowUpdate] = useState(false)
    const [showRemove, setShowRemove] = useState(false)
    const [showAddUser, setShowAddUser] = useState(false)
    const [showRemoveUser, setShowRemoveUser] = useState(false)


    const userId = useTypedSelector(store => store.user.user?.id)
    const owner = useTypedSelector(store => store.chat.chats.find(el => el.id === option.id!)?.creatorId === userId)
    const removeOrLeave = owner ? "Remove" : "Leave"

    const addUserIco = <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" className="bi mb-1 me-1 bi-person-add" viewBox="0 0 16 16">
        <path d="M12.5 16a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Zm.5-5v1h1a.5.5 0 0 1 0 1h-1v1a.5.5 0 0 1-1 0v-1h-1a.5.5 0 0 1 0-1h1v-1a.5.5 0 0 1 1 0Zm-2-6a3 3 0 1 1-6 0 3 3 0 0 1 6 0ZM8 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z" />
        <path d="M8.256 14a4.474 4.474 0 0 1-.229-1.004H3c.001-.246.154-.986.832-1.664C4.484 10.68 5.711 10 8 10c.26 0 .507.009.74.025.226-.341.496-.65.804-.918C9.077 9.038 8.564 9 8 9c-5 0-6 3-6 4s1 1 1 1h5.256Z" />
    </svg>

    const removeUserIco = <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" className="bi mb-1 me-1 bi-person-dash" viewBox="0 0 16 16">
        <path d="M12.5 16a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7ZM11 12h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1 0-1Zm0-7a3 3 0 1 1-6 0 3 3 0 0 1 6 0ZM8 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z" />
        <path d="M8.256 14a4.474 4.474 0 0 1-.229-1.004H3c.001-.246.154-.986.832-1.664C4.484 10.68 5.711 10 8 10c.26 0 .507.009.74.025.226-.341.496-.65.804-.918C9.077 9.038 8.564 9 8 9c-5 0-6 3-6 4s1 1 1 1h5.256Z" />
    </svg>

    const bucket = <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" className="bi mb-1 me-1 bi-trash" viewBox="0 0 16 16">
        <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5Zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5Zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6Z" />
        <path d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1ZM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118ZM2.5 3h11V2h-11v1Z" />
    </svg>

    const updateChatIcon = <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" className="mb-1 me-1 bi bi-pencil" viewBox="0 0 16 16">
        <path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z" />
    </svg>



    return <div className={`${option.show ? '' : 'd-none'} option d-flex flex-column py-3 position-absolute`} ref={ref}>

        {owner ? <>
            <div className='text-success' onClick={() => setShowUpdate(true)} >{updateChatIcon} Update</div>
            <div className='' onClick={() => setShowAddUser(true)}>{addUserIco} Add user</div>
            <div className='' onClick={() => setShowRemoveUser(true)}>{removeUserIco}Remove user</div>
            <div className='text-danger' onClick={() => setShowRemove(true)}>{bucket} {removeOrLeave}</div>
            <UpdateChat chatId={option.id!} show={showUpdate} setShow={setShowUpdate} />
            <AddUserToChat chatId={option.id!} show={showAddUser} setShow={setShowAddUser} />
            <RemoveFromChat chatId={option.id!} show={showRemoveUser} setShow={setShowRemoveUser} />
            <RemoveChat chatId={option.id!} show={showRemove} setShow={setShowRemove} /></>
            :
            <>
                <div className='text-danger' onClick={() => setShowRemove(true)}>{bucket} {removeOrLeave}</div>
                <LeaveFromChat chatId={option.id!} show={showRemove} setShow={setShowRemove} />
            </>}
    </div>
})