import React, { useState, useEffect, useRef } from 'react';
import { Container, Row, Col, Form } from 'react-bootstrap';
import { ConnectToChat } from '../Requests/Requests';
import '../Styles/App.css';
import 'bootstrap/dist/css/bootstrap.min.css'
import { addMessageMutation, queryGetAllChats, queryUser, subscriptionToChat, subscriptionToNotification } from '../Features/Queries';
import Dispatch from '../SocketDispatcher';
import { GetAbbreviationFromPhrase, GetStringFromDateTime, setCookie } from '../Features/Functions';
import MessageComponent from '../Components/Message';
import MultiControl from './MultiControl';
import ChatSelect from './ChatSelect';
import { useTypedSelector, useTypedDispatch } from '../Redux/store';
import { queryGetAllMessages, RequestBuilder } from '../Features/Queries';
import { AddChat } from './ChatOperation/AddChat';
import { Status, dropCurrentChat, setState as setStateChat } from '../Redux/Slicers/ChatSlicer';
import ChatHeader from './ChatHeader';
import NotificationModalWindow, { MessageType } from './Service/NotificationModalWindow';
import { setState as setStateGlobalNotification  } from '../Redux/Slicers/GlobalNotification';

export const maxVisibleLength = 26;

function Chat() {

  const currentChat = useTypedSelector(store => store.chat.currentChat)
  const messages = useTypedSelector(store => store.chat.messages)
  const user = useTypedSelector(store => store.user.user)
  const chatState = useTypedSelector(store => store.chat.status)
  const erroMessage = useTypedSelector(store => store.chat.error)
  const globalNotification = useTypedSelector(store => store.global_notification)
  const dispatch = useTypedDispatch()
  const [chatNotifyId, setChatNorifyId] = useState<string>("")
  const [userNotifyId, setUserNotifyId] = useState<string>("")
  let lastMessage = useRef<HTMLDivElement>(null);

  useEffect(() => {
    lastMessage.current?.scrollIntoView()
  }, [messages.length])

  useEffect(() => {
    setCookie({ name: "refresh_sent", value: "false" })
    const connection = ConnectToChat()
    connection.subscribe(sub => sub.subscribe({
      next: (response) => Dispatch(response)
    }))
    const subToNotify = RequestBuilder('start', { query: subscriptionToNotification })
    setUserNotifyId(subToNotify.id!)
    connection.subscribe(sub => sub.next(subToNotify))
    connection.subscribe(sub => sub.next(RequestBuilder('start', { query: queryUser })))
    connection.subscribe(sub => sub.next(RequestBuilder('start', { query: queryGetAllChats })))
  }, [])

  useEffect(() => {
    if (currentChat) {
      const connection = ConnectToChat()

      const chatIdVard = {
        chatId: currentChat.id
      }

      connection.subscribe(sub => sub.next(RequestBuilder('start', { query: queryGetAllMessages, variables: chatIdVard })))

      const subToChat = RequestBuilder('start', { query: subscriptionToChat, variables: chatIdVard });
      setChatNorifyId(subToChat.id!)

      connection.subscribe(sub => sub.next(subToChat))

      return () => {
        connection.subscribe(sub => sub.next(RequestBuilder('stop', {}, chatNotifyId)))
      }
    }
  }, [currentChat?.id])

  const SendMessage = (createdMessage: string, setCreatedMessage: React.Dispatch<string>) => {
    if (user && currentChat) {
      const connection = ConnectToChat()
      connection.subscribe(sub => sub.next(RequestBuilder('start', {
        query: addMessageMutation,
        variables: {
          chatId: currentChat.id,
          message: {
            content: createdMessage,
            sentAt: new Date()
          }
        }
      })));
    }
    setCreatedMessage('')
  }

  const DropChat = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Escape') {
      dispatch(dropCurrentChat())
    }
  }

  return <><Container onKeyDown={DropChat} tabIndex={0} fluid={'fluid'} className='chat-bg p-0 h-100 m-0'>
    <Row className='m-0 flex-row p-0 h-100'>
      <Col sm={3} className='chat-list h-100 p-3 select-chat-scroll border-end border-warning'>
        <AddChat />
        <ChatSelect />
      </Col>
      {currentChat?.id ?? -1 > 0 ?
        <Col className='d-flex flex-column h-100 p-0 m-0'>
          <ChatHeader currentChat={currentChat!} withChatInfo={true} />
          <Col className='p-4 m-0 h5 scroll'>
            {
              messages.map((el, key) => {
                return <MessageComponent key={el.id} el={el} ref={key == messages.length - 1 ? lastMessage : undefined} />
              })
            }
          </Col>
          <MultiControl SendMessage={SendMessage} />
        </Col>
        : null}
    </Row>
    <NotificationModalWindow innerText={erroMessage ?? ""} isShowed={isError(chatState)} messageType={MessageType.Error} dropMessage={() => {
      dispatch(setStateChat('idle'))
    }} />
    <NotificationModalWindow innerText={globalNotification.error ?? ""} isShowed={isError(globalNotification.status)} messageType={MessageType.Error} dropMessage={() => {
      dispatch(setStateGlobalNotification('idle'))
    }} />
  </Container >
  </>
}

export default Chat;

export function isSuccess(state: Status) {
  return state === 'success'
}

export function isError(state: Status) {
  return state === 'error'
}

export function isPadding(state: Status) {
  return state === 'padding'
}