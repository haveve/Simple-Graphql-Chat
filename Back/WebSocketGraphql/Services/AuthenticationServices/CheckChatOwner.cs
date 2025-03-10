using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;
using WebSocketGraphql.Services;
using WebSocketGraphql.Configurations;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    public class AuthHelper
    {
        private readonly int _iterations;

        public AuthHelper(IOptions<HashingSettings> settings)
            => _iterations = settings.Value.Iteration;

        public bool CheckChatOwner(int chatId, IDictionary<string, object?> authUser)
        {
            var data = authUser["UserOwn"] as string;

            if (data is null)
            {
                return false;
            }

            var ownChats = JsonSerializer.Deserialize<IEnumerable<int>>(data);

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

        public string GetUserNickName(IDictionary<string, object?> data)
        {
            return data.First(c => c.Key == "UserNickName").Value as string ?? throw new InvalidCastException("Nick was not found");
        }

        public bool IsAccess(IEnumerable<Claim> data)
        {
            try
            {
                var claimAccess = data.First(el => el.Type.Equals("IsAccess"));
                return Convert.ToBoolean(claimAccess.Value);
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<Claim> GetImmutableClaims(IEnumerable<Claim> data)
        {
            return data.Where(el => el.Type == "UserId");
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
            return guid.ComputeHash(guid, _iterations).Replace('/', '_').Replace('+', '-');
        }
    }
}
