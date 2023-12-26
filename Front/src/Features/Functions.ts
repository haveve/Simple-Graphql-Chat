import { Message, ReduxMessage } from "./Types";
import { redirect } from "react-router";
import { maxVisibleLength } from "../Components/Chat";
import { ChatParticipant } from "./Types";
import { ReduxParticipant } from "../Redux/Slicers/ChatSlicer";
import { Status } from "../Redux/Slicers/ChatSlicer";

export function SetMessageId(message: Message | ReduxMessage) {
  const date = new Date(message.sentAt).toISOString()
  message.id = `${date}.${message.fromId}.${message.chatId}`
}

export function GetStringFromDateTime(date: Date) {
  const dateN = new Date(date)
  return dateN.toISOString()
}


export function randomIntFromInterval(min: number, max: number) {
  return Math.floor(Math.random() * (max - min + 1) + min)
}

export function SortByOnline(data: ChatParticipant[] | ReduxParticipant[], desc?: boolean) {
  data.sort((a, b) => {
    const aOnline = a.online ? 1 : 0
    const bOnline = b.online ? 1 : 0
    const result = (bOnline - aOnline)
    return desc ? -result : result
  })
  return data
}

export function GetAbbreviationFromPhrase(str: string) {
  let startStr = ""
  str.split(" ").forEach((el, key) => {
    if (key < 2)
      startStr += el[0].toUpperCase();
  })
  return startStr
}

export function GetDisplayedName(str: string) {
  if (str.length > maxVisibleLength) {
    return str.substring(0, maxVisibleLength) + '...'
  }
  return str
}

export function GetDate(currentdate: Date){
  return currentdate.getDate() + "/"
  + (currentdate.getMonth() + 1) + "/"
  + currentdate.getFullYear() + ' '
}

export function GetFullDateTime(currentdate: Date) {
  return GetDate(currentdate)
    + TimeStringFromDate(currentdate)
}

export function TimeStringFromDate(date: Date): string {
  const hours = date.getHours();
  const minutes = date.getMinutes();
  return `${hours < 10 ? 0 : ''}${hours}:${minutes < 10 ? 0 : ''}${minutes}`
}

export function IsWhiteSpaceOrEmpty(str: string) {
  return str.trim() === ''
}

export function SortMessageByTime(messages: Message[], reverse: boolean = false) {
  const sign = reverse ? -1 : 1
  messages.sort((el1, el2) => {
    return sign * (el1.sentAt.getTime() - el2.sentAt.getTime())
  })
}

export function DateFromString(str: string) {
  return new Date(str)
}

export function LogoutDeleteCookie() {
  deleteCookie("refresh_token");
  deleteCookie("access_token");
  deleteCookie("user_id");
  deleteCookie("canUseUserIp");
  deleteCookie("refresh_sent");
}


export function getCookie(name: string) {
  name = name + "=";
  let ca = document.cookie.split(';');
  for (let i = 0; i < ca.length; i++) {
    let c = ca[i];
    while (c.charAt(0) == ' ') {
      c = c.substring(1);
    }
    if (c.indexOf(name) == 0) {
      return decodeURIComponent(c.substring(name.length, c.length));
    }
  }
  return null;
}

export function setCookie(cookieParams: setCookieParamas) {
  let s = cookieParams.name + '=' + encodeURIComponent(cookieParams.value) + ';';
  if (cookieParams.expires_second) {
    let d = new Date();
    d.setTime(d.getTime() + cookieParams.expires_second * 1000);
    s += ' expires=' + d.toUTCString() + ';';
  }
  if (cookieParams.path) s += ' path=' + cookieParams.path + ';';
  if (cookieParams.domain) s += ' domain=' + cookieParams.domain + ';';
  if (cookieParams.secure) s += ' secure;';
  document.cookie = s;
}

export function deleteCookie(name: string) {
  document.cookie = name + '=; expires=' + Date();
}

export function getTokenOrNavigate(isLoginRedirect: boolean = false) {
  const token = getCookie("refresh_token");
  if (!token && !isLoginRedirect) {
    return redirect("/");
  } else if (isLoginRedirect && token)
    return redirect("/main");
  return token;
}

export type setCookieParamas = {
  name: string,
  value: string,
  expires_second?: number,
  path?: string,
  domain?: string,
  secure?: boolean
}