using System.Security.Claims;
using System.Text.Json;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.Repositories;
using TimeTracker.Services;
using WebSocketGraphql.Helpers;
using WebSocketGraphql.Repositories;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    public class AuthHelper
    {
        private readonly IChat _chat;
        private readonly IConfiguration _configuration;

        public AuthHelper(IChat chat,IConfiguration configuration)
        {
            _chat = chat;
            _configuration = configuration;
        }

        public async Task<bool> CheckChatOwner(int userId, int chatId, IDictionary<string, object?> authUser)
        {
            var ownChats = JsonSerializer.Deserialize<IEnumerable<int>>(authUser["UserOwn"] as string);

            if (ownChats is null)
            {
                return false;
            }

            if (ownChats.Any(el => el == chatId))
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
            return authUser.Claims.First(c => c.Type == "UserNickName").Value;
        }

        public bool IsAccess(IEnumerable<Claim> data)
        {
            try
            {
                return Convert.ToBoolean(data.First(el => el.Type.Equals("isAccess")));
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<Claim> GetImmutableClaims(IEnumerable<Claim> data)
        {
            return data.Where(el => el.Type == "UserNickName" || el.Type == "UserId");
        }

        public IEnumerable<int>? GetChatParticipant(IDictionary<string, object?> data)
        {
            var owns = JsonSerializer.Deserialize<IEnumerable<int>>(data["UserOwn"] as string ?? "");
            var participants = JsonSerializer.Deserialize<IEnumerable<int>>(data["UserParticipated"] as string ?? "");

            var result = participants is null ? owns : owns?.Union(participants);

            return result;
        }

        public string GetRandomString()
        {
            var guid = Guid.NewGuid().ToString();
            return guid.ComputeHash(guid, _configuration.GetIteration());
        }
    }
}
