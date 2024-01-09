import React, { forwardRef, memo, useCallback } from "react";
import { TimeStringFromDate, DateFromString, GetDate } from "../Features/Functions";
import { useTypedSelector } from "../Redux/store";
import { baseUrl } from "./Icon";
import { defaultSmallPostfix, splitImage } from "../Features/Constants";

const MessageComponent = memo(forwardRef<HTMLDivElement, { HandleContext: (chatId: number, messageId: string, event: React.MouseEvent<HTMLDivElement>) => void, setDate: Boolean, id: string, className?: string }>((props, ref) => {
  const { id, HandleContext, setDate, className } = props

  const el = useTypedSelector(store => store.chat.messages.find(el => el.id == id)!)

  const onContextMenu = useCallback((event: React.MouseEvent<HTMLDivElement>) => {
    event.preventDefault();
    HandleContext(el.chatId, el.id!, event);
  }, [])

  const date = DateFromString(el.sentAt);

  const dateIndicator = setDate ? <div className="mb-3 message-date-color text-center">{GetDate(date)}</div> : null

  const slipImage = el.image?.split(splitImage);

  const currentId = useTypedSelector(store => store.user.user?.id)
  return <>{dateIndicator} {el.fromId ? <div ref={ref} className={currentId === el.fromId ? 'd-flex mb-3 justify-content-end' : 'mb-3'} >
    <div onContextMenu={onContextMenu} className={` ${className} max-width-30per d-inline-block message-style`}>
      <div className='d-flex justify-content-between gap-5 mb-2 '>
        <span className={`text-${currentId === el.fromId ? 'success' : 'info'}`}>
          {currentId === el.fromId ? 'You' : el.nickName}
        </span>
        <span className='text-end pt-2 text-primary time-font flex-grow-0 flex-shrink-0 flex-basis-0 h5 p-0 m-0'>
          {TimeStringFromDate(date)}
        </span>
      </div>
      {el.image ? <div className="w-100 mb-2 blur-load" style={{ backgroundImage: `url(${baseUrl + '/' + slipImage![0] + defaultSmallPostfix + splitImage + slipImage![1]})` }}>
        <img className="w-100 img" loading="lazy" src={baseUrl + '/' + el.image}></img>
      </div> : null}
      <div className='message-text-color'>
        {el.content.split('\n').map((mes, id) => <span key={el.id + '.' + id}>{mes}<br /></span>)}
      </div>
    </div>
  </div> : <div ref={ref} className="mb-3 message-tech-color text-center">{el.content}</div>
  }</>
}))

export default MessageComponent;