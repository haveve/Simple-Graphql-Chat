import React, { useState, useEffect, useRef, useMemo, memo, forwardRef } from 'react';
import { useTypedSelector, useTypedDispatch } from '../../Redux/store';
import { setUpdateMessage } from '../../Redux/Slicers/ChatSlicer';
import RemoveMessage from './RemoveMessage';
import { useTranslation } from 'react-i18next';

export type MessageOptionType = {
    show: boolean,
    messageId?: string
    chatId?: number
}

const Options = forwardRef<HTMLDivElement, { option: MessageOptionType }>((props, ref) => {
    const { option } = props;
    const [showRemove, setShowRemove] = useState(false)
    const dispatch = useTypedDispatch()
    const userId = useTypedSelector(el => el.user.user?.id)
    const message = useTypedSelector(store => store.chat.messages.find(el => el.id === option.messageId))

    const { t } = useTranslation();

    const bucket = <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="bi mb-1 me-1 bi-trash" viewBox="0 0 16 16">
        <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5Zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5Zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6Z" />
        <path d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1ZM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118ZM2.5 3h11V2h-11v1Z" />
    </svg>

    const updateMessageIcon = <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="mb-1 me-1 bi bi-pencil" viewBox="0 0 16 16">
        <path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z" />
    </svg>

    const canChange = option.show && userId === message?.fromId;

    const removeHandler = canChange ? () => setShowRemove(true) : undefined
    const updateHandler = canChange ? () => dispatch(setUpdateMessage(message!)) : undefined

    return <div className={`${canChange ? '' : 'd-message-none'} small chat-info-option d-flex flex-column py-3 position-absolute`} ref={ref}>
        <div className='text-danger' onClick={removeHandler}>{bucket} {t('Remove')}</div>
        <div className='text-success mt-2' onClick={updateHandler}>{updateMessageIcon} {t('Update')}</div>
        <RemoveMessage setShow={() => setShowRemove(false)} messageId={option.messageId!} show={showRemove} chatId={option.chatId!} />
    </div>
})

export default Options