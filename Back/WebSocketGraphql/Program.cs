using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.GraphQl.Schemes;
using TimeTracker.GraphQL.Schemas;
using TimeTracker.Repositories;
using TimeTracker.Services;
using WebSocketGraphql.Services.AuthenticationServices;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Validation.Rules;
using WebSocketGraphql.GraphQl.ValidationRules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IChat, Chat>();
builder.Services.AddSingleton<IAuthorizationManager, AuthorizationManager>();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<IAuthorizationRepository, AuthorizationRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddSingleton<AuthHelper>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<ISchema, ChatSchema>(service =>
{
    var schema = new ChatSchema(new SelfActivatingServiceProvider(service));
    return schema;
});

builder.Services.AddSingleton<ISchema, IdentitySchema>(service =>
{
    return new IdentitySchema(new SelfActivatingServiceProvider(service));
});

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddScheme<JwtBearerOptions, CustomJwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, options => { });

builder.Services.AddAuthorization();

builder.Services.AddCors();

builder.Services.AddGraphQL(c =>
{
    c.AddSystemTextJson()
    .AddSchema<ChatSchema>()
    .AddSchema<IdentitySchema>()
    .AddGraphTypes(typeof(ChatSchema).Assembly)
    .AddGraphTypes(typeof(IdentitySchema).Assembly)
    .AddValidationRule<CustomAuthorizationValidationRule>()
    .AddValidationRule<InputFieldsAndArgumentsOfCorrectLength>()
    .AddValidationRule<InputAndArgumentEmailValidationRule>()
    .AddWebSocketAuthentication<CustomWebSocketAuthenticator>()
    .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true);
});

var app = builder.Build();

app.UseCors(conf =>
{
    conf.WithOrigins(app.Configuration["FrontUrl"])
    .WithMethods("POST")
    .AllowAnyHeader()
    .AllowCredentials();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.UseGraphQL<ChatSchema>("/graphql");

app.UseGraphQL<IdentitySchema>("/graphql-auth");

app.UseGraphQLAltair();

app.Run();