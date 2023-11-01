import React, { forwardRef } from "react";
import { Message } from "../Features/Types";
import { GetFullDateTime } from "../Features/Functions";

const MessageComponent = forwardRef<HTMLDivElement,{currentId:number,el:Message}>((props,ref)=>{
    const {currentId,el} = props
    return <div ref={ref} className={currentId === parseInt(el.from.id) ? 'd-flex mb-3 justify-content-end' : 'mb-3'} >
      <div className={currentId === parseInt(el.from.id) ? 'max-width-50per text-success d-inline-block bg-white p-3 rounded' : 'max-width-50per text-primary d-inline-block p-3 rounded'}>
        <div className='d-flex justify-content-between gap-5 mb-2 border-bottom'>
          <span>
            {currentId === parseInt(el.from.id) ? 'You' : el.from.displayName}
          </span>
          <span className='text-end pt-2 text-primary time-font'>
            {GetFullDateTime(el.sentAt)}
          </span>
        </div>
        <div className='text-dark chat-transfer'>
          {el.content}
        </div>
      </div>
    </div>
})

export default MessageComponent;