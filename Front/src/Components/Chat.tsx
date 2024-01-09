import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Container, Row, Col, Form } from 'react-bootstrap';
import { ConnectToChat } from '../Requests/Requests';
import { addMessageMutation, queryGetAllChats, queryUser, subscriptionToChat, subscriptionToNotification } from '../Features/Queries';
import MessageComponent from '../Components/Message';
import MultiControl from './MultiControl';
import ChatSelect from './ChatSelect';
import { useTypedSelector, useTypedDispatch } from '../Redux/store';
import { queryGetAllMessages, RequestBuilder, updateMessageMutation } from '../Features/Queries';
import { AddChat } from './ChatOperation/AddChat';
import { Status, dropCurrentChat, dropUpdateMessage, setState, setState as setStateChat } from '../Redux/Slicers/ChatSlicer';
import ChatHeader from './ChatHeader';
import NotificationModalWindow, { MessageType } from './Service/NotificationModalWindow';
import { setState as setStateGlobalNotification } from '../Redux/Slicers/GlobalNotification';
import AdditionalInfo from './UserManagementAndAuthReg/AdditionalInfo';
import MessageOptions, { MessageOptionType } from './MessageOperation/MessageOption';
import { defaultState } from './ChatSelect';
import { useImmer } from 'use-immer';
import { MessageInput } from '../Features/Types';
import UseEffectIf from './Hooks/useEffectIf';
import { selectMessageIdsWithDate } from '../Redux/reselect';
import { DateFromString } from '../Features/Functions';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCameraAlt } from '@fortawesome/free-solid-svg-icons';
import AddMessageWithImage from './MessageOperation/AddMessageWithImage';
import { ajaxUploadFile } from '../Requests/Requests';
import { setError } from '../Redux/Slicers/ChatSlicer';
import { useTranslation } from 'react-i18next';

export const maxVisibleLength = 26;
export const maxSymbolsInMessage = 200;
enum themes {
  light = "light",
  dark = "dark"
}
export const MessagesInOneRequest = 15;


function Chat() {

  const chatPending = setState('pending');

  const currentChat = useTypedSelector(store => store.chat.currentChat)
  const messageIdsWithDate = useTypedSelector(selectMessageIdsWithDate)
  const chatState = useTypedSelector(store => store.chat.status)
  const erroMessage = useTypedSelector(store => store.chat.error)
  const globalNotification = useTypedSelector(store => store.global_notification)
  const dispatch = useTypedDispatch()
  const wasInitialAuth = useRef(true);
  let lastMessage = useRef<HTMLDivElement>(null);
  let refToOpt = useRef<HTMLDivElement>(null);

  const { t } = useTranslation()
  const prevDate = useRef<string | null>(null)

  const file = useRef<File | null>(null)
  const [message, setMessage] = useState('')

  const maxMessageHistoryFetchDate = useTypedSelector(store => store.chat.maxMessageHistoryFetchDate)
  const noHistoryMessagesLost = useTypedSelector(store => store.chat.noHistoryMessagesLost)
  const skip = useRef<number>(0)
  const rootChat = useRef<HTMLDivElement>(null)
  const intersectObservable = useRef<IntersectionObserver | null>(null)
  const firstMessage = useRef<HTMLDivElement>(null)

  const [theme, setTheme] = useState<themes>(themes.light)
  const [showSendMessageWithPict, setShowSendMessageWithPict] = useState(false)


  const [option, setOption] = useImmer<MessageOptionType>(defaultState)

  const updatedMessage = useTypedSelector(store => store.chat.updatedMessage)


  useEffect(() => {
    if (wasInitialAuth.current) {
      const connection = ConnectToChat(false, wasInitialAuth.current)
      const subToNotify = RequestBuilder('start', { query: subscriptionToNotification })

      connection.subscribe(sub => sub.next(subToNotify, chatPending))
      connection.subscribe(sub => sub.next(RequestBuilder('start', { query: queryUser }), chatPending))
      connection.subscribe(sub => sub.next(RequestBuilder('start', { query: queryGetAllChats }), chatPending))
    }
    return () => {
      wasInitialAuth.current = false;
    }
  }, [])


  useEffect(() => {
    const className = `root-${theme}`;
    document.body.classList.add(className)
    return () => {
      document.body.classList.remove(className)
    }
  }, [theme])


  UseEffectIf(() => {
    lastMessage.current?.scrollIntoView()
  },
    [messageIdsWithDate.length, skip.current], [0, 0],
    ([prevLength, prevSkip]) => {
      return prevSkip === 0 || prevLength + 1 === messageIdsWithDate.length
    })


  UseEffectIf(() => {
    if (firstMessage.current) {
      intersectObservable.current?.observe(firstMessage.current)
    }
  },
    [messageIdsWithDate.length], [0, 0],
    ([prevLength]) => {
      return prevLength + 1 !== messageIdsWithDate.length
    })



  useEffect(() => {
    if (noHistoryMessagesLost) {
      intersectObservable.current?.disconnect()
      intersectObservable.current = null;
    }
  }, [noHistoryMessagesLost])



  useEffect(() => {
    if (currentChat?.id && maxMessageHistoryFetchDate) {
      intersectObservable.current = new IntersectionObserver(entries => {
        const entry = entries[0]
        if (entry.isIntersecting) {
          const chatIdVard = {
            chatId: currentChat!.id,
            take: MessagesInOneRequest,
            skip: skip.current,
            maxDate: maxMessageHistoryFetchDate
          }
          const connection = ConnectToChat()
          connection.subscribe(sub => sub.next(RequestBuilder('start', { query: queryGetAllMessages, variables: chatIdVard }), chatPending))
          intersectObservable.current!.unobserve(entry.target)
          skip.current += MessagesInOneRequest;
        }
      }, { root: rootChat.current });

      if (firstMessage.current)
        intersectObservable.current!.observe(firstMessage.current)

      return () => {
        intersectObservable.current?.disconnect()
        intersectObservable.current = null;
      }
    }

  }, [currentChat?.id, maxMessageHistoryFetchDate])



  useEffect(() => {
    if (currentChat) {
      skip.current = 0;
      const connection = ConnectToChat()

      const chatIdVard = {
        chatId: currentChat!.id,
        take: MessagesInOneRequest,
        skip: skip.current,
        maxDate: null,
      }

      connection.subscribe(sub => sub.next(RequestBuilder('start', { query: queryGetAllMessages, variables: chatIdVard }), chatPending))
      const subToChat = RequestBuilder('start', { query: subscriptionToChat, variables: chatIdVard });
      connection.subscribe(sub => sub.next(subToChat, chatPending))

      return () => {
        connection.subscribe(sub => sub.next(RequestBuilder('stop', {}, subToChat.id!), chatPending))
      }
    }
  }, [currentChat?.id])

  const SendMessage = useCallback((createdMessage: string) => {

    if (updatedMessage) {
      const connection = ConnectToChat()
      const message: MessageInput = {
        content: createdMessage,
        sentAt: updatedMessage.sentAt
      }
      connection.subscribe(sub => sub.next(RequestBuilder('start', {
        query: updateMessageMutation,
        variables: {
          chatId: updatedMessage.chatId,
          message
        }
      }), chatPending))
      dispatch(dropUpdateMessage())
      return;
    }


    if (currentChat) {
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
      }), chatPending));
    }
  }, [currentChat, updatedMessage])


  const sendMessageWithPicture = useCallback((content: string, file: File) => {
    if (currentChat) {
      try {
        ajaxUploadFile(file, "image", addMessageMutation, {
          chatId: currentChat.id,
          message: {
            content,
            sentAt: new Date()
          }
        }).subscribe(_ => {
          dispatch(setError(t('DefaultErrorMessage')))
        })
      }
      catch (error) {
        const strError = error as string
        if (strError) {
          setError(strError)
        }
      }
    }
  }, [currentChat])

  const HandleContextMenu = useCallback((chatId: number, messageId: string, event: React.MouseEvent<HTMLDivElement>) => {
    event.preventDefault()
    if (refToOpt.current) {

      const elementData = refToOpt.current.getBoundingClientRect()

      refToOpt.current.style.top = event.clientY + "px"
      refToOpt.current.style.left = event.clientX - elementData.width + "px"
    }
    setOption(el => {
      el.chatId = chatId
      el.messageId = messageId
      el.show = true
    })
  }, [])

  const handleHideOption = () => setOption(el => {
    el.show = false
  })

  const DropChat = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Escape' && updatedMessage) {
      dispatch(dropUpdateMessage())
      return;
    }

    if (event.key === 'Escape') {
      dispatch(dropCurrentChat())
      return;
    }
  }

  const GetMessages = () => {
    prevDate.current = null;
    return messageIdsWithDate.map((data, key) => {

      const setDate = prevDate.current ? DateFromString(prevDate.current).getDate() !== DateFromString(data.sentAt).getDate() : true;

      if (setDate) {
        prevDate.current = data.sentAt
      }

      return <MessageComponent className={updatedMessage?.id === data.id ? "border border-5 border-info" : undefined} setDate={setDate} HandleContext={HandleContextMenu} key={data.id} id={data.id!} ref={key == messageIdsWithDate.length - 1 ? lastMessage : key === 0 ? firstMessage : undefined} />

    })
  }

  const pictureSelector = <><label htmlFor="selec-file-message" className='selec-file-message d-flex justify-content-center align-items-center h-100 w-100'>
    <FontAwesomeIcon icon={faCameraAlt}></FontAwesomeIcon>
  </label>
    <input type="file" onClick={(event) => {
      event.currentTarget.value = ""
    }} accept="image/*" hidden id="selec-file-message" onChange={event => {
      if (event.target.validity.valid && event.target.files && event.target.files[0]) {
        const img = event.target.files[0]
        setShowSendMessageWithPict(true);
        file.current = img;
      }
    }} /></>


  return <><div className="w-100 mb-2 h-100 blur-load position-relative">
    <Container onKeyDown={DropChat} tabIndex={0} fluid={'fluid'} className='position-absolute chat-bg p-0 h-100 m-0'>
      <Row className='m-0 flex-row p-0 h-100'>
        <Col sm={3} className='chat-list h-100 p-3 select-chat-scroll'>
          <div className="d-flex chat-size-head align-items-center p-0 m-0">
            <AddChat />
            <AdditionalInfo />
          </div>
          <ChatSelect />
        </Col>
        {currentChat?.id ?? -1 > 0 ?
          <Col className='d-flex flex-column h-100 p-0 m-0' onMouseLeave={handleHideOption} onClick={handleHideOption}>
            <ChatHeader onSmileClick={() => {
              setTheme(theme => theme == themes.dark ? themes.light : themes.dark)
            }} currentChat={currentChat!} withChatInfo={true} />
            <Col ref={rootChat} className='p-4 m-0 h5 scroll' onScroll={handleHideOption}>
              {
                GetMessages()
              }
            </Col>
            <MessageOptions option={option} ref={refToOpt}></MessageOptions>
            <MultiControl children={pictureSelector} maxSymbols={maxSymbolsInMessage} value={updatedMessage?.content} SendMessage={SendMessage} parentState={{ setState: setMessage, state: message }} />
          </Col>
          : null}
      </Row>
      <NotificationModalWindow innerText={erroMessage ?? ""} isShowed={isError(chatState)} messageType={MessageType.Error} dropMessage={() => {
        dispatch(setStateChat('idle'))
      }} />
      <NotificationModalWindow innerText={globalNotification.error ?? ""} isShowed={isError(globalNotification.status)} messageType={MessageType.Error} dropMessage={() => {
        dispatch(setStateGlobalNotification('idle'))
      }} />
      {currentChat && file.current ? <AddMessageWithImage handleSubmit={sendMessageWithPicture} chatId={currentChat?.id} file={file.current} show={showSendMessageWithPict} setShow={setShowSendMessageWithPict} setMessage={setMessage} message={message} /> : null}
    </Container >
    <img alt="" className='img main-img-load' />
  </div>
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
  return state === 'pending'
}