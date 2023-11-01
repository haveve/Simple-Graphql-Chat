export type MessageAdd = {
    fromId:string,
    content:string,
    sentAt:Date
}

export type From = {
    id:string,
    displayName:string
}

export type Message = {
    id?:string
    content:string,
    sentAt:Date,
    from:From
}