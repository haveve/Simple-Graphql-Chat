import React, { forwardRef } from "react";
import { ReduxMessage } from "../Features/Types";
import { GetFullDateTime, StringToDate } from "../Features/Functions";
import { useTypedSelector } from "../Redux/store";


const MessageComponent = forwardRef<HTMLDivElement,{el:ReduxMessage}>((props,ref)=>{
    const {el} = props
    const currentId = useTypedSelector(store => store.user.user?.id)
    return <div ref={ref} className={currentId === el.fromId ? 'd-flex mb-3 justify-content-end' : 'mb-3'} >
      <div className={currentId === el.fromId ? 'max-width-50per text-success d-inline-block bg-white p-3 rounded' : 'max-width-50per text-primary d-inline-block p-3 rounded'}>
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
    </div>
})

export default MessageComponent;