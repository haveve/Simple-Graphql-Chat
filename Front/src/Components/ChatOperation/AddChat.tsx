import React, { useState, useEffect, useRef } from 'react';
import { Container, Row, Col,Form } from 'react-bootstrap';
import MultiControl from '../MultiControl';
import { useTypedSelector } from '../../Redux/store';
import { createChatMutation } from '../../Features/Queries';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';

export function AddChat() {

    const user = useTypedSelector(store => store.user.user)
    const addChat = (chatName: string, setChatName: React.Dispatch<string>) => {
        if (user) {
          const connection = ConnectToChat()
          connection.subscribe(sub => sub.next(RequestBuilder('start', {
            query: createChatMutation,
            variables: {
                name: chatName,
            }
          })));
        }
        setChatName('')
      }

    return <MultiControl size='w-100' className='border-primary mb-2' placeHolder='Add chat' SendMessage={addChat} />

}