using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Authenticator;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using TimeTracker.Repositories;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebSocketGraphql.Services.AuthenticationServices;
using TimeTracker.Helpers;
using System.Text;

namespace TimeTracker.Controllers
{
    [Authorize]
    public class _2fAuth : Controller
    {
        [Route("2f-auth")]
        public async Task<IActionResult> Get2fData([FromServices] AuthHelper helper, [FromServices] IUserRepository userRepository)
        {
            string key = helper.GetRandomString();

            var id = helper.GetUserId(HttpContext.User);

            var user = await userRepository.GetUserAsync(id);

            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            SetupCode setupInfo = tfa.GenerateSetupCode(HttpContext.Request.Host.Host, user.Email, key, false, 3);

            string qrCodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            string manualEntrySetupCode = setupInfo.ManualEntryKey;

            return Json(new { qrUrl = qrCodeImageUrl, manualEntry = manualEntrySetupCode, key });
        }


        public class RequestData
        {
            public string Key { get; set; }
            public string Code { get; set; }
        }

        [HttpPost("set-2f-auth")]
        public async Task<IActionResult> SetUser2fAuth([FromServices] AuthHelper helper, [FromServices] IAuthorizationRepository authorizationRepository, [BindRequired][FromBody] RequestData reqData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Uncorrect Code");
            }

            var id = helper.GetUserId(HttpContext.User);
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            var resetCode = helper.GetRandomString();

            var data = tfa.ValidateTwoFactorPIN(reqData.Key, reqData.Code);

            if (data && await authorizationRepository.Add2factorKeyAsync(id, reqData.Key, resetCode))
            {
                return Json(resetCode);
            }

            return BadRequest("Uncorrect Code");
        }

        [AllowAnonymous]
        [Route("verify-2f-auth")]
        public async Task<IActionResult> Verify2fAuth([FromServices] IUserRepository userRepository, [FromServices] IAuthorizationManager authorizationManager, [FromServices] IAuthorizationRepository _authorizationRepository, [FromQuery] string token, [FromQuery] string code)
        {
            if (!await authorizationManager.IsValidToken(token,refresh:true))
            {
                return BadRequest("Invalid temporary token");
            }

            var tokenData = authorizationManager.ReadJwtToken(token);
            try
            {
                int userId = int.Parse(tokenData.Claims.First(c => c.Type == "UserId").Value);
                string refreshToken = tokenData.Claims.First(c => c.Type == "RefreshToken").Value;
                DateTime issuedAt = JsonSerializer.Deserialize<DateTime>(tokenData.Claims.First(c => c.Type == "IssuedAtRefresh").Value);
                DateTime expiredAt = JsonSerializer.Deserialize<DateTime>(tokenData.Claims.First(c => c.Type == "ExpiredAtRefresh").Value);

                var user = await userRepository.GetUserAsync(userId);

                if (user is null || (!_2fAuthHelper.Has2fAuth(user) && !_2fAuthHelper.Check2fAuth(user.Key2Auth, code)))
                {
                    return BadRequest("Invalid one-time code or you does not turn on 2f auth");
                }

               await _authorizationRepository.CreateRefreshTokenAsync(new(refreshToken, expiredAt, issuedAt), user.Id);

                Dictionary<string, string?> queryParams = new Dictionary<string, string?>
            {
                { "expiredAt", JsonSerializer.Serialize(expiredAt)},
                { "issuedAt", JsonSerializer.Serialize(issuedAt) },
                { "token", refreshToken }
            };

                var url = QueryHelpers.AddQueryString("/", queryParams);

                return Json(url);
            }
            catch
            {
                return BadRequest("Invalid temporary token");
            }

        }


        [Route("drop-2f-auth")]
        public async Task<IActionResult> Drop2fAuth([FromServices] AuthHelper helper, [FromServices] IConfiguration config, [FromServices] IAuthorizationRepository authorizationRepository, [BindRequired][FromQuery] string code)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid parameters");
            }

            var id = helper.GetUserId(HttpContext.User);
            string? key = await authorizationRepository.Get2factorKeyAsync(id);

            if (key != null && _2fAuthHelper.Check2fAuth(key, code) && await authorizationRepository.Drop2factorKeyAsync(id, null))
            {
                return Ok("2f auth was droped");
            }
            else if (await authorizationRepository.Drop2factorKeyAsync(id, code))
            {
                return Ok("2f auth was droped");
            }

            return BadRequest("Invalid one-time code or you does not turn on 2f auth");
        }
    }
}
