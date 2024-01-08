import React, { useState, useEffect, useRef } from 'react';
import MultiControl from '../MultiControl';
import { useTypedSelector } from '../../Redux/store';
import { createChatMutation } from '../../Features/Queries';
import { ConnectToChat } from '../../Requests/Requests';
import { RequestBuilder } from '../../Features/Queries';
import { setState } from '../../Redux/Slicers/ChatSlicer';
import { useTranslation } from 'react-i18next';

export function AddChat() {

  const chatPending = setState('pending');

  const { t } = useTranslation();

  const user = useTypedSelector(store => store.user.user)
  const addChat = (chatName: string) => {
    if (user) {
      const connection = ConnectToChat()
      connection.subscribe(sub => sub.next(RequestBuilder('start', {
        query: createChatMutation,
        variables: {
          name: chatName,
        }
      }), chatPending));
    }
  }

  return <MultiControl maxSymbols={100} size='w-100' className='border-primary mb-2' placeHolder={t('AddChat')} SendMessage={addChat} />

}