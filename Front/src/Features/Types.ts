

export type MessageAdd = {
    content: string,
    sentAt: Date
}

export type MessageInput = {
    content: string,
    sentAt: string
}

export type ParticipantState = {
    online: boolean,
    nickName: string | null,
    id: number
}

export type DerivedMessageOrChatInfo = ParticipantState & (Message & { typeM: MessageType })
export type DrivedUserOrChatInfo = (Chat & { typeC: ChatResultType }) & UserNotification
export type BaseMessage = {
    id?: string
    content: string,
    image: string | null,
    fromId: number | null,
    chatId: number,
    nickName: string
}

export type ChatParticipant = {
    id: number,
    nickName: string,
    online: boolean,
    avatar: string | null
}

export type ChatUpdate = {
    name: string,
    id: number
}

export type ReduxMessage = BaseMessage & { sentAt: string }

export type Message = BaseMessage & { sentAt: Date, deleteAll?: boolean }

export type Chat = {
    id: number,
    name: string,
    creatorId: number,
    avatar?: string | null
}

export type FullChat = Chat & {
    chatMembersCount: number
}

export type UserNotification = {
    id: number,
    notificationType: ChatNotificationType,
    name: string,
    creatorId: number,
    userId: number
}

export type User = {
    id: number,
    nickName: string,
    email: string,
    key2Auth: string | null,
    avatar: string | null
}

export enum MessageType {
    CREATE = "CREATE",
    UPDATE = "UPDATE",
    DELETE = "DELETE",
    USER_ADD = "USER_ADD",
    USER_REMOVED = "USER_REMOVE"
}

export enum ChatResultType {
    UPDATE = "UPDATE",
    DELETE = "DELETE"
}

export enum ChatNotificationType {
    ENROLL = "ENROLL",
    BANISH = "BANISH"
}