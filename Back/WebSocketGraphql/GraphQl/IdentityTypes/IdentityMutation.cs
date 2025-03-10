using Google.Authenticator;
using GraphQL;
using GraphQL.Types;
using WebSocketGraphql.Helpers;
using WebSocketGraphql.Models;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.Services;
using WebSocketGraphql.GraphQl.IdentityTypes.Models;
using WebSocketGraphql.Services.AuthenticationServices;
using WebSocketGraphql.GraphQl.Directives.Validation;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class IdentityMutation : ObjectGraphType
    {
        public const int minPasswordLength = 8;
        public const int maxPasswordLength = 24;

        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;

        public IdentityMutation(IUserRepository userRepository, IEmailSender emailSender, IAuthorizationRepository authorizationRepository, AuthHelper helper)
        {
            _userRepository = userRepository;
            _emailSender = emailSender;

            Field<NonNullGraphType<StringGraphType>>("registration")
                .Argument<NonNullGraphType<RegistrationInputGraphType>>("registration")
                .ResolveAsync(async context =>
                {
                    Registration UserData = context.GetArgument<Registration>("registration");
                    User user = new();
                    user.NickName = UserData.NickName;
                    user.Email = UserData.Email;
                    string code = await _userRepository.CreateUserAsync(user);

                    await _emailSender.SendRegistrationEmailAsync(code, UserData.Email);

                    return "Ok";
                });

            Field<NonNullGraphType<StringGraphType>>("sentResetPasswordEmail")
                .Argument<StringGraphType>("nickNameOrEmail", el => el.RestrictLength(RegistrationInputGraphType.minNickNameLength, RegistrationInputGraphType.maxNickNameLength))
                .ResolveAsync(async context =>
                {
                    string LoginOrEmail = context.GetArgument<string>("nickNameOrEmail");
                    User? user = await _userRepository.GetUserByNickNameOrEmailAsync(LoginOrEmail);
                    if (user == null)
                    {
                        context.Errors.Add(new ExecutionError("User was not found!"));
                        return null;
                    }

                    string code = EmailSendHelper.GetUniqueCode();

                    await _userRepository.UpdateUserResetCodeByIdAsync(user.Id, code);

                    await _emailSender.SendResetPassEmailAsync(code, user.Email);

                    return "Email has sent!";
                });
            Field<NonNullGraphType<StringGraphType>>("resetUserPasswordByCode")
               .Argument<NonNullGraphType<StringGraphType>>("code")
               .Argument<NonNullGraphType<StringGraphType>>("password", el => el.RestrictLength(minPasswordLength, maxPasswordLength))
               .Argument<NonNullGraphType<StringGraphType>>("email", el => el
                    .RestrictLength(RegistrationInputGraphType.minEmailLength, RegistrationInputGraphType.maxEmailLength)
                    .RestrictAsEmail())
               .ResolveAsync(async context =>
               {
                   string code = context.GetArgument<string>("code");
                   string password = context.GetArgument<string>("password");
                   string email = context.GetArgument<string>("email");
                   User? user = await _userRepository.GetUserByNickNameOrEmailAsync(email);
                   if (user == null)
                   {
                       context.Errors.Add(new ExecutionError("User not found"));
                       return null;
                   }

                   if (user.ActivateCode == null)
                   {
                       context.Errors.Add(new ExecutionError("User was not requesting password change"));
                       return null;
                   }

                   if (user.ActivateCode != code)
                   {
                       context.Errors.Add(new ExecutionError("Reset code not match"));
                       return null;
                   }

                   await _userRepository.UpdateUserPasswordAndCodeAsync(user.Id, password);

                   return "Password reseted successfully";
               });

            Field<NonNullGraphType<StringGraphType>>("set2fAuth")
            .Argument<NonNullGraphType<Set2fDataInputGraphType>>("data")
            .ResolveAsync(async (context) =>
            {

                var reqData = context.GetArgument<Set2fData>("data");

                var id = helper.GetUserId(context.User!);
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                var resetCode = helper.GetRandomString();

                var data = tfa.ValidateTwoFactorPIN(reqData.Key, reqData.Code);

                if (data && await authorizationRepository.Add2factorKeyAsync(id, reqData.Key, resetCode))
                {
                    return resetCode;
                }

                ThrowError("Uncorrect Code");

                //pointless, but compiler point on it if i don't return value
                return null;
            }).AuthorizeWithPolicy("Authorized");

            Field<NonNullGraphType<StringGraphType>>("drop2fAuth")
            .Argument<NonNullGraphType<StringGraphType>>("code")
            .ResolveAsync(async (context) =>
            {

                var code = context.GetArgument<string>("code");

                var id = helper.GetUserId(context.User!);
                string? key = await authorizationRepository.Get2factorKeyAsync(id);

                if (key != null && _2fAuthHelper.Check2fAuth(key, code) && await authorizationRepository.Drop2factorKeyAsync(id, null))
                {
                    return "2f auth was droped";
                }
                else if (await authorizationRepository.Drop2factorKeyAsync(id, code))
                {
                    return "2f auth was droped";
                }

                ThrowError("Invalid one-time code or you does not turn on 2f auth");

                //pointless, but compiler point on it if i don't return value
                return null;

            }).AuthorizeWithPolicy("Authorized");

        }

        private void ThrowError(string message)
        {
            throw new InvalidDataException(message);
        }
    }
}
