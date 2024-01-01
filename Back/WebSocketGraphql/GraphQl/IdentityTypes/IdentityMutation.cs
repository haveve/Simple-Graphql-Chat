using GraphQL;
using GraphQL.Types;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.Models;
using TimeTracker.Repositories;
using TimeTracker.Services;
using WebSocketGraphql.GraphQl.IdentityTypes.Models;
using WebSocketGraphql.Services;
using WebSocketGraphql.Services.AuthenticationServices;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class IdentityMutation : ObjectGraphType
    {
        public const int minPasswordLength = 8;
        public const int maxPasswordLength = 24;

        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;
        private readonly AuthHelper _helper;

        public IdentityMutation(IUserRepository userRepository, IEmailSender emailSender, AuthHelper helper)
        {

            _userRepository = userRepository;
            _emailSender = emailSender;
            _helper = helper;

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
                .Argument<StringGraphType>("nickNameOrEmail", el => el.ApplyDirective(
                   "length", "min", RegistrationInputGraphType.minNickNameLength, "max", RegistrationInputGraphType.maxEmailLength))
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
               .Argument<NonNullGraphType<StringGraphType>>("password", el => el.ApplyDirective(
                "length", "min", minPasswordLength, "max", maxPasswordLength))
               .Argument<NonNullGraphType<StringGraphType>>("email", el => el.ApplyDirective(
                "length", "min", RegistrationInputGraphType.minEmailLength, "max", RegistrationInputGraphType.maxEmailLength)
                .ApplyDirective("email"))
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


        }
    }
}
