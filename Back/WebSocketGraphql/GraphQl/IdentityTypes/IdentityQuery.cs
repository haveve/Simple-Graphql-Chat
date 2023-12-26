using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.WebUtilities;
using TimeTracker.GraphQL.Types.IdentityTipes;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;
using TimeTracker.Helpers;
using TimeTracker.Models;
using TimeTracker.Repositories;
using TimeTracker.Services;
using WebSocketGraphql.GraphQl.IdentityTypes;
using WebSocketGraphql.GraphQl.IdentityTypes.Models;
using WebSocketGraphql.Services;

namespace TimeTracker.GraphQL.Queries
{
    public class IdentityQuery : ObjectGraphType
    {
        private readonly IAuthorizationManager _authorizationManager;
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;


        public IdentityQuery(IConfiguration configuration, IAuthorizationManager authorizationManager, IAuthorizationRepository authorizationRepository, IUserRepository userRepository, IEmailSender emailSender)
        {
            _authorizationManager = authorizationManager;
            _authorizationRepository = authorizationRepository;
            _configuration = configuration;
            _userRepository = userRepository;
            _emailSender = emailSender;

            Field<IdentityOutPutGraphType>("login")
                .Argument<NonNullGraphType<LoginInputGraphType>>("login")
            .ResolveAsync(async context =>
            {
                Login UserLogData = context.GetArgument<Login>("login");
                var userRepository = context.RequestServices.GetService<IUserRepository>();

                var user = await _userRepository.GetUserByCredentialsAsync(UserLogData.NickNameOrEmail, UserLogData.Password);
                
                if (user == null)
                {
                    throw new Exception("User does not exist");
                }

                if(user.ActivateCode != null)
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
                        refresh_token = new(string.Empty,DateTime.MinValue, DateTime.MinValue),
                        redirect_url = QueryHelpers.AddQueryString("/2f-auth", tempQueryParams)
                    };

                }

                var encodedJwt =  await _authorizationManager.GetAccessToken(user.Id);

                var refreshToken = _authorizationManager.GetRefreshToken(user.Id);
                _authorizationRepository.CreateRefreshToken(refreshToken, user.Id);

                var response = new LoginOutput()
                {
                    access_token = encodedJwt,
                    user_id = user.Id,
                    refresh_token = refreshToken
                };

                return response;
            });

            Field<IdentityOutPutGraphType>("refreshToken").
                ResolveAsync(async(context) =>
                {
                    HttpContext httpContext = context.RequestServices!.GetService<IHttpContextAccessor>()!.HttpContext!;

                    var refreshToken = httpContext.Request.Headers.First(at => at.Key == "refresh_token").Value[0]!;

                    if (refreshToken == null)
                    {
                        return ExpiredSessionError(context);
                    }

                    var whetherValid = await _authorizationManager.ValidateRefreshToken(refreshToken);

                    if (!whetherValid.isValid)
                    {
                        _authorizationRepository.DeleteRefreshToken(refreshToken);
                        return ExpiredSessionError(context);
                    }

                    int userId = int.Parse(_authorizationManager.ReadJwtToken(refreshToken).Claims.First(c => c.Type == "UserId").Value);
                    var newRefreshToken = _authorizationManager.GetRefreshToken(userId);

                    _authorizationRepository.UpdateRefreshToken(refreshToken, newRefreshToken, userId);

                    return new LoginOutput()
                    {
                        access_token = whetherValid.accessToken!,
                        user_id = userId,
                        refresh_token = newRefreshToken
                    };

                });

            Field<StringGraphType>("logout").
              Resolve((context) =>
              {
                  HttpContext httpContext = context.RequestServices!.GetService<IHttpContextAccessor>()!.HttpContext!;
                  var refreshToken = httpContext.Request.Headers.First(at => at.Key == "refresh_token").Value[0]!;

                  _authorizationRepository.DeleteRefreshToken(refreshToken);

                  return "Successfully";
              });
        }

        public LoginOutput ExpiredSessionError(IResolveFieldContext<object?> context)
        {
            context.Errors.Add(new ExecutionError("User does not auth"));
            return new LoginOutput()
            {
                access_token = new("",new DateTime(),new DateTime()),
                user_id = 0,
                refresh_token = new("Your session was expired. Please, login again", new DateTime(), new DateTime()),
            };
        }
    }
}
