using GraphQL;
using GraphQL.Types;
using TimeTracker.GraphQL.Types.IdentityTipes;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;
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

            Field<string>("registration")
                .Argument<NonNullGraphType<RegistrationInputGraphType>>("registration")
                .ResolveAsync( async context =>
                {
                    Registration UserData = context.GetArgument<Registration>("registration");
                    User user = new();
                    user.NickName = UserData.NickName;
                    user.Email = UserData.Email;
                    string code = await _userRepository.CreateUserAsync(user);

                    _emailSender.SendRegistrationEmail(code, UserData.Email);

                    return "Ok";
                });

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

            Field<StringGraphType>("sentResetPasswordEmail")
                .Argument<StringGraphType>("nickNameOrEmail")
                .ResolveAsync(async context =>
                {
                    string LoginOrEmail = context.GetArgument<string>("nickNameOrEmail");
                    User? user = await _userRepository.GetUserByNickNameOrEmailAsync(LoginOrEmail);
                    if (user == null)
                        return "User was not found!";

                    string code = EmailSendHelper.GetUniqueCode();

                    await _userRepository.UpdateUserResetCodeByIdAsync(user.Id, code);

                    _emailSender.SendResetPassEmail(code, user.Email);

                    return "Email has sent!";
                });
            Field<StringGraphType>("resetUserPasswordByCode")
               .Argument<NonNullGraphType<StringGraphType>>("code")
               .Argument<NonNullGraphType<StringGraphType>>("password")
               .Argument<NonNullGraphType<StringGraphType>>("email")
               .ResolveAsync(async context =>
               {
                   string code = context.GetArgument<string>("code");
                   string password = context.GetArgument<string>("password");
                   string email = context.GetArgument<string>("email");
                   User? user = await _userRepository.GetUserByNickNameOrEmailAsync(email);
                   if (user == null) return "User not found";
                   if (user.ActivateCode == null) return "User was not requesting password change";
                   if (user.ActivateCode != code) return "Reset code not match";

                   await _userRepository.UpdateUserPasswordAndCodeAsync(user.Id, password);

                   return "Password reseted successfully";
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
