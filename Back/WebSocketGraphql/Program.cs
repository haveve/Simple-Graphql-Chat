using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.AuthorizationManager;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.GraphQl.Schemes;
using WebSocketGraphql.GraphQL.Schemas;
using WebSocketGraphql.Services;
using WebSocketGraphql.Services.AuthenticationServices;
using GraphQL.Validation.Rules;
using WebSocketGraphql.GraphQl.ValidationRules;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.Configurations;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IChat, Chat>();
builder.Services.AddSingleton<IAuthorizationManager, AuthorizationManager>();
builder.Services.AddSingleton<IUploadRepository, UploadRepository>();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<IAuthorizationRepository, AuthorizationRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddSingleton<AuthHelper>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOptionsWithValidation<HashingSettings>(builder.Configuration.GetSection("PasswordHashing"));
builder.Services.AddOptionsWithValidation<SpaSettings>(builder.Configuration.GetSection("Spa"));
builder.Services.AddOptionsWithValidation<GeneralSettings>(builder.Configuration.GetSection("GeneralSettings"));
builder.Services.AddOptionsWithValidation<AuthorizationSettings>(builder.Configuration.GetSection("Authorization"));
builder.Services.AddOptionsWithValidation<EmailSettings>(builder.Configuration.GetSection("Email"));

builder.Services.AddGraphQLUpload();

builder.Services.AddSingleton<ISchema, ChatSchema>(service =>
{
    var schema = new ChatSchema(new SelfActivatingServiceProvider(service));
    return schema;
});

builder.Services.AddSingleton<ISchema, IdentitySchema>(service =>
{
    return new IdentitySchema(new SelfActivatingServiceProvider(service));
});

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddScheme<JwtBearerOptions, CustomJwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, null);

builder.Services.AddAuthorization();

builder.Services.AddCors();

builder.Services.AddGraphQL(c =>
{
    c.AddSystemTextJson()
    .AddSchema<ChatSchema>()
    .AddSchema<IdentitySchema>()
    .AddGraphTypes(typeof(ChatSchema).Assembly)
    .AddGraphTypes(typeof(IdentitySchema).Assembly)
    .AddAuthorization(conf =>
    {
        conf.AddPolicy("Authorized", p => p.RequireAuthenticatedUser());
    })
    .AddValidationRule<CustomAuthorizationValidationRule>()
    .AddValidationRule<InputFieldsAndArgumentsOfCorrectLength>()
    .AddValidationRule<InputAndArgumentEmailValidationRule>()
    .AddValidationRule<InputAndArgumentNumberValidationRule>()
    .AddWebSocketAuthentication<CustomWebSocketAuthenticator>()
    .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true);
});

var app = builder.Build();

app.UseCors(conf =>
{
    conf.WithOrigins(app.Configuration["Spa:Url"])
        .WithMethods("POST")
        .AllowAnyHeader()
        .AllowCredentials();
});

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.UseGraphQLUpload<ChatSchema>("/graphql");

app.UseGraphQL<ChatSchema>("/graphql");

app.UseGraphQL<IdentitySchema>("/graphql-auth");

app.UseGraphQLAltair();

app.Run();


public static class ServiceExtensions
{
    public static OptionsBuilder<TOptions> AddOptionsWithValidation<TOptions>(this IServiceCollection services, IConfiguration config) where TOptions : class
    {
        return services.AddOptions<TOptions>()
            .Bind(config)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}