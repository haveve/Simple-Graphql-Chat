using System.Text.Json;
using Google.Authenticator;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.WebUtilities;
using WebSocketGraphql.GraphQL.Types.IdentityTipes;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.AuthorizationManager;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.Models;
using WebSocketGraphql.Helpers;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.GraphQl.IdentityTypes;
using WebSocketGraphql.GraphQl.IdentityTypes.Models;
using WebSocketGraphql.Services.AuthenticationServices;

namespace WebSocketGraphql.GraphQL.Queries
{
    public class IdentityQuery : ObjectGraphType
    {
        private readonly IAuthorizationManager _authorizationManager;
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        public IdentityQuery(IConfiguration configuration, IHostEnvironment hostEnvironment, IAuthorizationManager authorizationManager, IAuthorizationRepository authorizationRepository, IUserRepository userRepository, AuthHelper helper)
        {
            _authorizationManager = authorizationManager;
            _authorizationRepository = authorizationRepository;
            _configuration = configuration;
            _userRepository = userRepository;

            Field<IdentityOutPutGraphType>("login")
                .Argument<NonNullGraphType<LoginInputGraphType>>("login")
                .ResolveAsync(async context =>
                {
                    Login UserLogData = context.GetArgument<Login>("login");

                    var user = await _userRepository.GetUserByCredentialsAsync(UserLogData.NickNameOrEmail, UserLogData.Password);

                    if (user == null)
                    {
                        throw new Exception("User does not exist");
                    }

                    if (user.ActivateCode != null)
                    {
                        throw new Exception("User has not setted password");
                    }

                    if (user.Key2Auth != null)
                    {
                        var refresh_2f_tokens = authorizationManager.GetRefreshToken(user.Id);

                        var tempToken = _2fAuthHelper.GetTemporaty2fAuthToken(user, refresh_2f_tokens, _configuration["Authorization:Issuer"], _configuration["Authorization:Audience"], _configuration["Authorization:Key"]);

                        Dictionary<string, string?> tempQueryParams = new Dictionary<string, string?>
                {
                    { "tempToken", tempToken }
                };
                        return new LoginOutput()
                        {
                            access_token = new(string.Empty, DateTime.MinValue, DateTime.MinValue),
                            user_id = -1,
                            refresh_token = new(string.Empty, DateTime.MinValue, DateTime.MinValue),
                            redirect_url = QueryHelpers.AddQueryString("/2f-auth", tempQueryParams)
                        };

                    }

                    var encodedJwt = await _authorizationManager.GetAccessToken(user.Id);

                    var refreshToken = _authorizationManager.GetRefreshToken(user.Id);
                    await _authorizationRepository.CreateRefreshTokenAsync(refreshToken, user.Id);

                    var response = new LoginOutput()
                    {
                        access_token = encodedJwt,
                        user_id = user.Id,
                        refresh_token = refreshToken
                    };

                    return response;
                });

            Field<IdentityOutPutGraphType>("refreshToken").
                ResolveAsync(async (context) =>
                {
                    var httpContext = context.RequestServices!.GetService<IHttpContextAccessor>()!.HttpContext!;

                    var refreshToken = httpContext.Request.Headers.First(at => at.Key == "refresh_token").Value[0]!;

                    if (refreshToken == null)
                    {
                        return ExpiredSessionError(context);
                    }

                    var whetherValid = await _authorizationManager.ValidateRefreshToken(refreshToken);

                    if (!whetherValid.isValid)
                    {
                        await _authorizationRepository.DeleteRefreshTokenAsync(refreshToken);
                        return ExpiredSessionError(context);
                    }

                    int userId = int.Parse(_authorizationManager.ReadJwtToken(refreshToken).Claims.First(c => c.Type == "UserId").Value);
                    var newRefreshToken = _authorizationManager.GetRefreshToken(userId);

                    await _authorizationRepository.UpdateRefreshTokenAsync(refreshToken, newRefreshToken, userId);

                    return new LoginOutput()
                    {
                        access_token = whetherValid.accessToken!,
                        user_id = userId,
                        refresh_token = newRefreshToken
                    };

                });

            Field<StringGraphType>("logout").
              ResolveAsync(async (context) =>
              {
                  HttpContext httpContext = context.RequestServices!.GetService<IHttpContextAccessor>()!.HttpContext!;
                  var refreshToken = httpContext.Request.Headers.First(at => at.Key == "refresh_token").Value[0]!;

                  await _authorizationRepository.DeleteRefreshTokenAsync(refreshToken);

                  return "Successfully";
              });


            Field<Get2fDataOuputGraphType>("get2fAuth")
                .ResolveAsync(async (context) =>
                {
                    string key = helper.GetRandomString();

                    var id = helper.GetUserId(context.User!);

                    var user = await userRepository.GetUserAsync(id);

                    TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                    SetupCode setupInfo = tfa.GenerateSetupCode(hostEnvironment.ApplicationName, user!.Email, key, false, 3);

                    string qrCodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                    string manualEntrySetupCode = setupInfo.ManualEntryKey;

                    return new Get2fData { QrUrl = qrCodeImageUrl, ManualEntry = manualEntrySetupCode, Key = key };
                }).AuthorizeWithPolicy("Authorized");

            Field<StringGraphType>("verify2fAuth")
            .Argument<NonNullGraphType<StringGraphType>>("token")
            .Argument<NonNullGraphType<StringGraphType>>("code")
            .ResolveAsync(async (context) =>
            {

                var token = context.GetArgument<string>("token");
                var code = context.GetArgument<string>("code");

                if (!authorizationManager.IsValidToken(token, refresh: true))
                {
                    ThrowError("Invalid temporary token");
                }

                var tokenData = authorizationManager.ReadJwtToken(token);
                try
                {
                    int userId = int.Parse(tokenData.Claims.First(c => c.Type == "UserId").Value);
                    string refreshToken = tokenData.Claims.First(c => c.Type == "RefreshToken").Value;
                    DateTime issuedAt = JsonSerializer.Deserialize<DateTime>(tokenData.Claims.First(c => c.Type == "IssuedAtRefresh").Value);
                    DateTime expiredAt = JsonSerializer.Deserialize<DateTime>(tokenData.Claims.First(c => c.Type == "ExpiredAtRefresh").Value);

                    var user = await userRepository.GetUserAsync(userId);
                    if (user is null || !_2fAuthHelper.Has2fAuth(user) || (!_2fAuthHelper.Check2fAuth(user.Key2Auth!, code) && user.ResetCode != code))
                    {
                        context.Errors.Add(new("Invalid one-time code or you does not turn on 2f auth"));
                        return null;
                    }

                    await _authorizationRepository.CreateRefreshTokenAsync(new(refreshToken, expiredAt, issuedAt), user.Id);

                    Dictionary<string, string?> queryParams = new Dictionary<string, string?>
            {
                { "expiredAt", JsonSerializer.Serialize(expiredAt)},
                { "issuedAt", JsonSerializer.Serialize(issuedAt) },
                { "token", refreshToken }
            };

                    var url = QueryHelpers.AddQueryString("", queryParams);

                    return url;
                }
                catch
                {
                    ThrowError("Invalid temporary token");

                    //never return, but compiler demand it
                    return null;
                }

            });
        }

        private void ThrowError(string message)
        {
            throw new InvalidDataException(message);
        }

        public LoginOutput ExpiredSessionError(IResolveFieldContext<object?> context)
        {
            context.Errors.Add(new ExecutionError("User does not auth"));
            return new LoginOutput()
            {
                access_token = new("", new DateTime(), new DateTime()),
                user_id = 0,
                refresh_token = new("Your session was expired. Please, login again", new DateTime(), new DateTime()),
            };
        }
    }
}
