import React, { useState, useEffect, useRef } from 'react';
import { Container, Row, Col } from 'react-bootstrap';
import { ConnectToChat } from '../Requests/Requests';
import '../Styles/App.css';
import 'bootstrap/dist/css/bootstrap.min.css'
import { addMessageMutation, queryUser, subscriptionToChat, subscriptionToNotification } from '../Features/Queries';
import Dispatch from '../SocketDispatcher';
import { GetDateStringFromDateTime, setCookie } from '../Features/Functions';
import MessageComponent from '../Components/Message';
import MultiControl from './MultiControl';
import ChatSelect from './ChatSelect';
import { useTypedSelector} from '../Redux/store';
import { queryGetAllMessages, RequestBuilder } from '../Features/Queries';

export const maxVisibleLength = 26;

function Chat() {

  const currentChat = useTypedSelector(store => store.chat.currentChat)
  const messages = useTypedSelector(store => store.chat.messages)
  const user = useTypedSelector(store => store.user.user)
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
  }, [])

  useEffect(() => {
    if (currentChat) {
      const connection = ConnectToChat()
      connection.subscribe(sub => sub.next(RequestBuilder('start', { query: queryGetAllMessages })))

      const subToChat = RequestBuilder('start', { query: subscriptionToChat });
      setChatNorifyId(subToChat.id!)

      connection.subscribe(sub => sub.next(subToChat))

      return () => {
        connection.subscribe(sub => sub.next(RequestBuilder('stop', {}, chatNotifyId)))
      }
    }
  }, [currentChat?.id])

  const SendMessage = (createdMessage: string, setCreatedMessage: React.Dispatch<string>) => {
    if (user) {
      const connection = ConnectToChat()
      connection.subscribe(sub => sub.next(RequestBuilder('start', {
        query: addMessageMutation,
        variables: {
          message: {
            content: createdMessage,
            sentAt: GetDateStringFromDateTime(new Date())
          }
        }
      })));
    }
    setCreatedMessage('')
  }

  return <><Container fluid={'fluid'} className='chat-bg p-0 h-100 m-0'>
    <Row className='m-0 flex-row p-0 h-100'>
      <Col sm={3} className='chat-list h-100 p-3 select-chat-scroll'>
        <ChatSelect />
      </Col>
      {currentChat?.id ?? -1 > 0 ?
        <Col className='d-flex flex-column h-100 p-0 m-0'>
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
  </Container >
  </>
}

export default Chat;
