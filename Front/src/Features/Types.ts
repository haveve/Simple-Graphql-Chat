export type MessageAdd = {
    content:string,
    sentAt:Date
}

export type DerivedMessageOrChatInfo = (Chat & {typeM:MessageType})&(Message&{typeC:ChatResultType})

export type BaseMessage = {
    id?:string
    content:string,
    fromId:number,
    chatId:number,
    nickName:string
}

export type ChatParticipant = {
    id:number,
    nickName:string
}

export type ReduxMessage = BaseMessage & {sentAt:string}

export type Message = BaseMessage & {sentAt:Date}

export type Chat = {
    id:number,
    name:string,
    creatorId:number,
    chatMembersCount:number
}

export type UserNotification = {
    id:number,
    notificationType:ChatNotificationType,
    name:string,
    creatorId:number,
    userId:number,
    chatMembersCount:number
}

export type User = {
    id:number,
    nickName:string,
    email:string
}

export enum MessageType 
{ 
   CREATE = "CREATE",
   UPDATE = "UPDATE",
   DELETE = "DELETE",
   USER_ADD = "USER_ADD",
   USER_REMOVED = "USER_REMOVE"
}

export enum ChatResultType
{
    UPDATE = "UPDATE",
    DELETE = "DELETE"
}

export enum ChatNotificationType
{
    ENROLL = "ENROLL",
    BANISH = "BANISH"
}