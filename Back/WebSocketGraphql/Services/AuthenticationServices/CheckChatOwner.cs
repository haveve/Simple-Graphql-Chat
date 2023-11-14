using System.Security.Claims;
using System.Text.Json;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.Repositories;
using WebSocketGraphql.Repositories;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    public class AuthHelper
    {
        private readonly IChat _chat;

        public AuthHelper(IChat chat)
        {
            _chat = chat;
        }

        public async Task<bool> CheckChatOwner(int userId, int chatId, ClaimsPrincipal authUser)
        {
            var ownChats = JsonSerializer.Deserialize<IEnumerable<int>>(authUser.Claims.First(c => c.ValueType == "UserOwn").Value);

            if (ownChats is null)
            {
                return false;
            }

            if(ownChats.Any(el=>el == chatId))
            {
                return true;
            }

            if ( await _chat.CheckUserOwnChat(userId, chatId))
            {
                return true;
            }

            return false;
        }

        public int GetUserId(ClaimsPrincipal authUser)
        {
           return JsonSerializer.Deserialize<int>(authUser.Claims.First(c => c.Type == "UserId").Value);
        }

        public string GetUserNickName(ClaimsPrincipal authUser) 
        {
            return authUser.Claims.First(c => c.ValueType == "UserNickName").Value;
        }
    }
}
