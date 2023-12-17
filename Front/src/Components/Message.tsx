import React, { forwardRef,memo,useCallback } from "react";
import { ReduxMessage } from "../Features/Types";
import { GetFullDateTime, ToNormalDate } from "../Features/Functions";
import { useTypedSelector } from "../Redux/store";


const MessageComponent = memo(forwardRef<HTMLDivElement,{HandleContext:(chatId:number,messageId:string,event:React.MouseEvent<HTMLDivElement>)=>void,id:string}>((props,ref)=>{
    const {id,HandleContext} = props

    const el = useTypedSelector(store => store.chat.messages.find(el => el.id == id)!)

    const onContextMenu = useCallback((event:React.MouseEvent<HTMLDivElement>)=>{
        event.preventDefault();
        HandleContext(el.chatId,el.id!,event);
    },[])

    const currentId = useTypedSelector(store => store.user.user?.id)
    return el.fromId?<div ref={ref} className={currentId === el.fromId ? 'd-flex mb-3 justify-content-end' : 'mb-3'} >
      <div onContextMenu = {onContextMenu} className={`max-width-50per text-${currentId === el.fromId ?'success':'info'} d-inline-block bg-white p-3 rounded`}>
        <div className='d-flex justify-content-between gap-5 mb-2 border-bottom'>
          <span>
            {currentId === el.fromId ? 'You' : el.nickName}
          </span>
          <span className='text-end pt-2 text-primary time-font'>
            {GetFullDateTime(ToNormalDate(el.sentAt))}
          </span>
        </div>
        <div className='text-dark chat-transfer'>
          {el.content}
        </div>
      </div>
    </div>:<div ref = {ref} className="mb-3 text-secondary text-center">{el.content}</div>
}))

export default MessageComponent;