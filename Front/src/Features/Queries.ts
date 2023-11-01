export const subscriptionToChat = `subscription{
    messageAdded{
      sentAt
      content
      from{
        id,
        displayName
      }
    }
  }`

export const queryGetAll = `{
    messages{
        sentAt
        content
        from{
            id
            displayName
        }
    }
  }`

export const addOneMessageMutation = `mutation($message:MessageInputType){
    addMessage(message:$message)
}`