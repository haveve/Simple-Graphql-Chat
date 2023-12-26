

export type MessageAdd = {
    content:string,
    sentAt:Date
}

export type MessageInput = {
    content:string,
    sentAt:string
}

export type ParticipantState = {
    online:boolean,
    id:number
}

export type DerivedMessageOrChatInfo = ParticipantState&(Chat & {typeM:MessageType})&(Message&{typeC:ChatResultType})

export type BaseMessage = {
    id?:string
    content:string,
    fromId:number|null,
    chatId:number,
    nickName:string
}

export type ChatParticipant = {
    id:number,
    nickName:string,
    online:boolean
}

export type ChatUpdate = {
    name:string,
    id:number
}

export type ReduxMessage = BaseMessage & {sentAt:string}

export type Message = BaseMessage & {sentAt:Date,deleteAll?:boolean}

export type Chat = {
    id:number,
    name:string,
    creatorId:number,
}

export type FullChat = Chat & {
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
    email:string,
    key2Auth:string|null
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