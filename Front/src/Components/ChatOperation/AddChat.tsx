import React, { useState, useEffect, useRef } from 'react';
import { Container, Row, Col,Form } from 'react-bootstrap';
import MultiControl from '../MultiControl';
import { useTypedSelector } from '../../Redux/store';
import { createChatMutation } from '../../Features/Queries';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import { setState } from '../../Redux/Slicers/ChatSlicer';

export function AddChat() {

    const chatPending = setState('pending');

    const user = useTypedSelector(store => store.user.user)
    const addChat = (chatName: string, setChatName: React.Dispatch<string>) => {
        if (user) {
          const connection = ConnectToChat()
          connection.subscribe(sub => sub.next(RequestBuilder('start', {
            query: createChatMutation,
            variables: {
                name: chatName,
            }
          }),chatPending));
        }
        setChatName('')
      }

    return <MultiControl size='w-100' className='border-primary mb-2' placeHolder='Add chat' SendMessage={addChat} />

}