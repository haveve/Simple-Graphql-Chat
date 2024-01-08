import { nanoid } from "nanoid"
import { defaultSubscriptionResponse } from "../Requests/Requests"

export interface OperationMessage {
  payload?: any,
  id: string,
  type: string
}

export type PayloadType = {
  query?: string,
  variables?: any,
  [name: string]: any
}
export type ConnectionType = 'connection_init' | 'start' | 'stop' | 'connection_terminate'

export function RequestBuilder(type: ConnectionType, payload: PayloadType = {}, id: string | null = null): defaultSubscriptionResponse<any> {
  id ??= nanoid()
  return { payload, id, type }
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
      sentAt,
      nickName,
      deleteAll,
      image
    }
    ...on ChatResulSubscription{
      typeC:type,
      name,
      id,
      creatorId,
      chatMembersCount,
    },
    ... on ChatParticipantSubscription{
      id,
      online,
      nickName
    }
  }
}`

//Query

export const queryGetAllMessages = `query($chatId:Int!,$take:Int!,$skip:Int!,$maxDate:DateTime){
  messages(chatId:$chatId,take:$take,skip:$skip,maxDate:$maxDate){
      sentAt
      content
      chatId
      fromId,
      nickName,
      image
  }
}`

export const queryGetAllChats = `query{
  chats{
    id,
    creatorId,
    name,
    avatar
  }
}`

export const queryParticipants = `query($chatId:Int!,$search:String){
  participants(chatId:$chatId,search:$search){
    id,
    nickName,
    online,
    avatar
  }
}`

export const queryUser = `query{
  user{
    id,
    email,
    nickName,
    key2Auth,
    avatar
  }
}`

export const queryFullChatInfo = `query($chatId:Int!){
  chatFullInfo(chatId:$chatId){
    name,
    id,
    creatorId,
    chatMembersCount,
    avatar
  }
}`

//Mutation

export const addMessageMutation = `mutation($message:MessageInput!,$chatId:Int!,$image:Upload){addMessage(message:$message,chatId:$chatId,image:$image){fromId,chatId,sentAt,content,nickName,image}}`

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
      chatMembersCount
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

export const removeUserFromChatMutation = `mutation($chatId:Int!,$user:String!,$deleteAll:Boolean){
    removeUserFromChat(chatId:$chatId,user:$user,deleteAll:$deleteAll)
  }`

export const leaveFromChatMutation = `mutation($chatId:Int!,$deleteAll:Boolean){
    leaveFromChat(chatId:$chatId,deleteAll:$deleteAll)
  }`

export const updateUserDataMutaion = `mutation update($data:UserUpdateInput!){
  updateUser(data:$data){
      email,
      nickName,
      id
    }
  }`

export const deleteUserMutation = `mutation remove($data:UserRemoveInput!){
    deleteUser(data:$data)
  }`

export const updateUserAvatarMutation = `mutation($file:Upload!){updateUserAvatart(image:$file)}`;

export const updateChatAvatarMutation = `mutation($chatId:Int!,$file:Upload!){updateChatAvatart(image:$file,chatId:$chatId)}`;


//2f

export const set2fMutation = `mutation set2f($data:Set2fDataInput!){
  set2fAuth(data:$data)
}`

export const drop2fMutation = `mutation drop2f($code:String!){
  drop2fAuth(code:$code)
}`

export const get2fQuery = `query get2f{
  get2fAuth{
    key,
    manualEntry,
    qrUrl
  }
}
`

export const verify2fQuery = `query varify2f($token:String!,$code:String!){
  verify2fAuth(token:$token,code:$code)
}`