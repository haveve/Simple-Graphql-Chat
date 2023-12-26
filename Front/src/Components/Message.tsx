import React, { forwardRef,memo,useCallback } from "react";
import { TimeStringFromDate, DateFromString,GetDate } from "../Features/Functions";
import { useTypedSelector } from "../Redux/store";


const MessageComponent = memo(forwardRef<HTMLDivElement,{HandleContext:(chatId:number,messageId:string,event:React.MouseEvent<HTMLDivElement>)=>void,setDate:Boolean,id:string}>((props,ref)=>{
    const {id,HandleContext,setDate} = props

    const el = useTypedSelector(store => store.chat.messages.find(el => el.id == id)!)

    const onContextMenu = useCallback((event:React.MouseEvent<HTMLDivElement>)=>{
        event.preventDefault();
        HandleContext(el.chatId,el.id!,event);
    },[])

    const date = DateFromString(el.sentAt);

    const dateIndicator = setDate?<div className="mb-3 message-date-color text-center">{GetDate(date)}</div>:null

    const currentId = useTypedSelector(store => store.user.user?.id)
    return<>{dateIndicator} {el.fromId?<div ref={ref} className={currentId === el.fromId ? 'd-flex mb-3 justify-content-end' : 'mb-3'} >
      <div onContextMenu = {onContextMenu} className={`max-width-50per text-${currentId === el.fromId ?'success':'info'} d-inline-block message-style`}>
        <div className='d-flex justify-content-between gap-5 mb-2 '>
          <span>
            {currentId === el.fromId ? 'You' : el.nickName}
          </span>
          <span className='text-end pt-2 text-primary time-font'>
            {TimeStringFromDate(date)}
          </span>
        </div>
        <div className='message-text-color chat-transfer'>
          {el.content}
        </div>
      </div>
    </div>:<div ref = {ref} className="mb-3 message-tech-color text-center">{el.content}</div>
}</>
}))

export default MessageComponent;