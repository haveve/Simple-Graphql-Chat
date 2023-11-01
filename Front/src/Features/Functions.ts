import { Message } from "./Types";

export function GetDateStringFromDateTime(date:Date){
    return date.toJSON();
}

export function GetFullDateTime(currentdate:Date){
    return currentdate.getDate() + "/"
    + (currentdate.getMonth()+1)  + "/" 
    + currentdate.getFullYear() + ' '
    + TimeStringFromDate(currentdate)
}

export function TimeStringFromDate(date: Date): string {
    const hours = date.getHours();
    const minutes = date.getMinutes();
    return `${hours < 10 ? 0 : ''}${hours}:${minutes < 10 ? 0 : ''}${minutes}`
}

export function MessagesNormalizeDateFormat(messages:Message[]){
    messages.forEach(el=>{
        el.sentAt = new Date(el.sentAt)
    })
}

export function IsWhiteSpaceOrEmpty(str:string){
    return str.trim() === ''
}

export function SortMessageByTime(messages:Message[],reverse:boolean = false){
    const sign = reverse?-1:1
    messages.sort((el1,el2)=>{
        return sign * (el1.sentAt.getTime() - el2.sentAt.getTime())
    })
}