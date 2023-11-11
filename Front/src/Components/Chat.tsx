import React, { useState, useEffect, useRef } from 'react';
import { Container, Row, Col } from 'react-bootstrap';
import { ConnectToChat } from '../Requests/Requests';
import '../Styles/App.css';
import 'bootstrap/dist/css/bootstrap.min.css'
import { Message } from '../Features/Types';
import { nanoid } from 'nanoid';
import { subscriptionToChat, queryGetAll, addOneMessageMutation } from '../Features/Queries';
import Dispatch, { QUERY_ALL, ADD_MESSAGE_ONE, RECEIVE_SUBSCRIBED_MESSAGE } from '../SocketDispatcher';
import { GetDateStringFromDateTime, MessagesNormalizeDateFormat, SortMessageByTime,GetAbbreviationFromPhrase,GetDisplayedName } from '../Features/Functions';
import { flushSync } from 'react-dom';
import MessageComponent from '../Components/Message';
import MultiControl from './MultiControl';
import { WebSocketProxy,defaultSubscriptionResponse,} from '../Requests/Requests';
import distinctColors from 'distinct-colors'
import ChatSelect from './ChatSelect';

export const maxVisibleLength = 26;

function Chat() {

  const [currentId, setId] = useState(-1);
  const [messages, setMessages] = useState<Message[]>([]);
  let lastMessage = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const connection = ConnectToChat()
    connection.subscribe({
      next: (response) => {
        const dispatched = Dispatch(response);
        switch (dispatched.dataType) {
          case RECEIVE_SUBSCRIBED_MESSAGE:
            let message = dispatched.data as Message;
            MessagesNormalizeDateFormat([message])
            flushSync(() => setMessages(data => [...data, message]))
            lastMessage.current!.scrollIntoView()
            break;
          case QUERY_ALL:
            let data = dispatched.data as Message[]
            MessagesNormalizeDateFormat(data)
            SortMessageByTime(data)
            flushSync(() => setMessages(data))
            lastMessage.current?.scrollIntoView()
            break;
          case ADD_MESSAGE_ONE:
            console.log(response.data)
            break;
        }
      }
    })
    connection.next({
      id: nanoid(),
      type: 'start',
      payload: {
        query: queryGetAll
      }
    })

    connection.next({
      id: nanoid(),
      type: 'start',
      payload: {
        query: subscriptionToChat
      }
    })

    return () => connection!.complete()
  }, [])

  const SendMessage = (createdMessage: string, setCreatedMessage: React.Dispatch<string>) => {
      const connection = ConnectToChat()
      connection!.next({
        id: nanoid(),
        type: 'start',
        payload: {
          query: addOneMessageMutation,
          variables: {
            message: {
              content: createdMessage,
              fromId: currentId.toString(),
              sentAt: GetDateStringFromDateTime(new Date())
            }
          }
        }
      }
      );
      setCreatedMessage('')
    }

  return <><Container fluid={'fluid'} className='chat-bg p-0 h-100 m-0'>
    <Row className='m-0 flex-row p-0 h-100'>
      <Col sm={3} className='chat-list h-100 p-3 select-chat-scroll'>
          <ChatSelect/>
      </Col>
      <Col className='d-flex flex-column h-100 p-0 m-0'>
        <Col className='p-4 m-0 h5 scroll'>
          {
            messages.map((el, key) => {
              return <MessageComponent key = {el.id} el={el} currentId={currentId} ref={key == messages.length - 1 ? lastMessage : undefined} />
            })
          }
        </Col>
        <MultiControl SendMessage={SendMessage} />
      </Col>
    </Row>
  </Container >
  </>
}

export default Chat;
