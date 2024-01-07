import React, { useState, useEffect, useRef, JSX } from 'react';
import { GetAbbreviationFromPhrase } from '../Features/Functions';
import { backDomain } from '../Features/Constants';

const baseUrl = `https://${backDomain}`

export default function Icon(props: { color: string, name: string, children?: JSX.Element | null, withChatInfo?: boolean, handleChatInfo?: () => void, onlyImage?: boolean, src?: string | null, onlyImageSmall?: boolean }) {

    const { color, name, children, withChatInfo, handleChatInfo, onlyImage, src, onlyImageSmall } = props;

    const userAbr = src ? null : <div className='position-absolute z-0 ico-color'>{GetAbbreviationFromPhrase(name)}</div>;

    const icoText = onlyImage && children ? <>
        {userAbr}
        <div className='z-2 w-100 h-100'>{children}</div>
    </>
        : userAbr

    const linkToImg = baseUrl + '/' + src

    const img = src ? <div className={`p-0 m-0 chat-title-icon-size flex-grow-0 flex-shrink-0 flex-basis-0 ${onlyImage && !onlyImageSmall ? 'chat-title-icon-size-xl' : null}`}
        style={{
            backgroundImage: `url(${linkToImg})`,
            backgroundRepeat: 'no-repeat',
            backgroundPosition: 'center',
            backgroundSize: 'contain'
        }}>
        {icoText}
    </div> :
        <div className={`chat-title-icon-size ${onlyImage && !onlyImageSmall ? 'chat-title-icon-size-xl' : null} flex-grow-0 flex-shrink-0 flex-basis-0 h5 p-0 m-0`} style={{
            backgroundColor: color,
        }}>
            {icoText}
        </div>


    return onlyImage ? img : <div className='d-flex ' role={withChatInfo ? "button" : "img"} onClick={handleChatInfo}>
        {img}
        <div className='h5 pt-1 ms-3 d-flex flex-grow-0 flex-shrink-1 flex-basis-1 flex-column justify-content-center align-items-start' >
            <span className='chat-text-color'>{name}</span>
            {children}
        </div >
    </div>
}