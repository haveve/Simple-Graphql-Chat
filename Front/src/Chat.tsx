import React, { useState, useEffect, useRef} from 'react';
import { Container, Row, Col, Form, Button } from 'react-bootstrap';
import { ConnectToChat} from './Requests';
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css'
import { Message } from './Types';
import { WebSocketSubject } from 'rxjs/webSocket'
import { nanoid } from 'nanoid';
import { subscriptionToChat, queryGetAll, addOneMessageMutation } from './Queries';
import Dispatch, { QUERY_ALL, ADD_MESSAGE_ONE, RECEIVE_SUBSCRIBED_MESSAGE } from './SocketDispatcher';
import { GetDateStringFromDateTime } from './Functions';
import { flushSync } from 'react-dom';

function Chat() {
  const [currentId, setId] = useState(-1);
  const [createdMessage, setCreatedMessage] = useState('')
  const [messages, setMessages] = useState<Message[]>([]);
  let connection = useRef<WebSocketSubject<any>>();
  let lastMessage = useRef<HTMLDivElement>(null);
  
  useEffect(() => {
    connection.current = ConnectToChat()
    connection.current.subscribe({
      next: (response) => {
        const dispatched = Dispatch(response);
        switch (dispatched.dataType) {
          case RECEIVE_SUBSCRIBED_MESSAGE:
            flushSync(()=>setMessages(data => [...data, dispatched.data!]))
            lastMessage.current!.scrollIntoView()
            break;
          case QUERY_ALL:
            setMessages(dispatched.data!)
            break;
          case ADD_MESSAGE_ONE:
            console.log(response.data)
            break;
        }
      }
    })
    connection.current.next({
      id: nanoid(),
      type: 'start',
      payload: {
        query: queryGetAll
      }
    })

    connection.current.next({
      id: nanoid(),
      type: 'start',
      payload: {
        query: subscriptionToChat
      }
    })

    return () => connection.current!.complete()
  }, [])

  const title = 'Awesome chat'
  const logo = <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" fill="white" className="bi bi-activity" viewBox="0 0 16 16">
    <path fillRule="evenodd" d="M6 2a.5.5 0 0 1 .47.33L10 12.036l1.53-4.208A.5.5 0 0 1 12 7.5h3.5a.5.5 0 0 1 0 1h-3.15l-1.88 5.17a.5.5 0 0 1-.94 0L6 3.964 4.47 8.171A.5.5 0 0 1 4 8.5H.5a.5.5 0 0 1 0-1h3.15l1.88-5.17A.5.5 0 0 1 6 2Z" />
  </svg>

  return <><Container fluid={'fluid'} className='p-0 m-0'>
    <Row className='bg-success p-3 m-0'>
      <Col>
        {logo}
        <span className='p-4 h4 text-white'>{title}</span>
      </Col>
    </Row>
    <Row className='m-0 p-0'>
      <Col className='m-4  h5'>
        {
          messages.map((el,key) => {
            return <div ref = {key == messages.length - 1?lastMessage:undefined} key={el.id!} className={currentId === parseInt(el.from.id) ? 'text-success mb-3' : 'mb-3'} ><span>{currentId === parseInt(el.from.id) ? 'You> ' : el.from.displayName + '> '}</span> <span>{el.content}</span></div>
          })
        }
      </Col>
    </Row>
    <Row fluid='fluid' className='justify-content-center p-0 m-0 mb-2 fixed-bottom'>
      <Row className='w-50 bg-success p-3 pe-0 m-0 rounded-1'>
        <Col sm='8'>
          <Form.Control placeholder='Message' onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
            setCreatedMessage(event.target.value);
          }}></Form.Control>
        </Col>
        <Col>
          <Form.Select onChange={(event: React.ChangeEvent<HTMLSelectElement>) => {
            const id = parseInt(event.target.value)
            if (id > 0) {
              setId(id)
            }
          }}>
            <option key={-1} value="-1">select</option>
            <option key={1} value="1">developer</option>
            <option key={2} value="2">tester</option>
          </Form.Select>
        </Col>
        <Col>
          <Button variant='secondary' onClick={() => {
            connection.current!.next({
              id: nanoid(),
              type: 'start',
              payload: {
                query: addOneMessageMutation,
                variables:{
                  message: {
                    content: createdMessage,
                    fromId: currentId.toString(),
                    sentAt: GetDateStringFromDateTime(new Date())
                  }
                }
              }
            });
          }}>Send</Button>
        </Col>
      </Row>
    </Row>
  </Container >
  </>
}

export default Chat;
