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

enum MessageType 
{ 
   CREATE = "CREATE",
   UPDATE = "UPDATE",
   DELETE = "DELETE"
}