import { nanoid } from "nanoid"
import { defaultSubscriptionResponse } from "../Requests/Requests"

export interface OperationMessage{
  payload?:any,
  id:string,
  type:string
}

export type PayloadType = {
  query?:string,
  variables?:any,
  [name:string]:any
}
export type ConnectionType = 'connection_init'|'start'|'stop'|'error'|'connection_keep_alive'|'connection_terminate'

export function RequestBuilder(type:ConnectionType,payload:PayloadType = {},id:string|null = null):defaultSubscriptionResponse<any>{
  id ??= nanoid()
  return {payload,id,type}
}

//Subscription

export const subscriptionToNotification = `subscription{
  userNotification{
    id,
    name,
    creatorId,
    notificationType,
    chatMembersCount
  }
}`

export const subscriptionToChat = `subscription($chatId:Int!){
  chatNotification(chatId:$chatId){
    ...on MessageSubscription{
      fromId,
      chatId,
      content,
      typeM:type,
      nickName
    }
    ...on ChatResulSubscription{
      typeC:type,
      name,
      id,
      creatorId,
      chatMembersCount,
    }
  }
}`

//Query

export const queryGetAllMessages = `query($chatId:Int!){
  messages(chatId:$chatId){
      sentAt
      content
      chatId
      fromId,
      nickName
  }
}`

export const queryGetAllChats = `query{
  chats{
    id,
    creatorId,
    chatMembersCount,
    name,
  }
}`

export const queryParticipants = `query($chatId:Int!){
  participats(chatId:$chatId){
    id,
    nickName
  }
}`

export const queryUser = `query{
  user{
    id,
    email,
    nickName
  }
}`

//Mutation

export const addMessageMutation = `mutation($message:MessageInput!,$chatId:Int!){
  addMessage(message:$message,chatId:$chatId){
    fromId,
    chatId,
    sentAt,
    content,
    nickName
  }
}`

export const removeMessageMutation = `mutation($message:MessageInput!,$chatId:Int!){
  removeMessage(message:$message,chatId:$chatId){
    fromId,
    chatId,
    sentAt,
    content,
    nickName
  }
}`

export const updateMessageMutation = `mutation($message:MessageInput!,$chatId:Int!){
  updateMessage(message:$message,chatId:$chatId){
      fromId,
      chatId,
      sentAt,
      content,
      nickName
  }  
  }`

  export const createChatMutation = `mutation($name:String!){
    createChat(name:$name){
      id,
      creatorId,
      name,
    }
  }`

  export const updateChatMutation = `mutation($chat:ChatInput!){
    updateChat(chat:$chat){
      name,
      id,
      creatorId
    }
  }`

  export const removeChatMutation = `mutation($chatId:Int!){
    removeChat(chatId:$chatId)
  }`

  export const addUserToChatMutation = `mutation($chatId:Int!,$user:String!){
    addUserToChat(chatId:$chatId,user:$user)
  }`

  export const removeUserFromChatMutation = `mutation($chatId:Int!,$user:String!){
    removeUserFromChat(chatId:$chatId,user:$user)
  }`