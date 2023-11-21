import React, { forwardRef } from "react";
import { ReduxMessage } from "../Features/Types";
import { GetFullDateTime, StringToDate } from "../Features/Functions";
import { useTypedSelector } from "../Redux/store";


const MessageComponent = forwardRef<HTMLDivElement,{el:ReduxMessage}>((props,ref)=>{
    const {el} = props
    const currentId = useTypedSelector(store => store.user.user?.id)
    return el.fromId?<div ref={ref} className={currentId === el.fromId ? 'd-flex mb-3 justify-content-end' : 'mb-3'} >
      <div className={`max-width-50per text-${currentId === el.fromId ?'success':'info'} d-inline-block bg-white p-3 rounded`}>
        <div className='d-flex justify-content-between gap-5 mb-2 border-bottom'>
          <span>
            {currentId === el.fromId ? 'You' : el.nickName}
          </span>
          <span className='text-end pt-2 text-primary time-font'>
            {GetFullDateTime(StringToDate(el.sentAt))}
          </span>
        </div>
        <div className='text-dark chat-transfer'>
          {el.content}
        </div>
      </div>
    </div>:<div ref = {ref} className="mb-3 text-secondary text-center">{el.content}</div>
})

export default MessageComponent;