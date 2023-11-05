using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.GraphQl.Schemes;
using TimeTracker.GraphQL.Schemas;
using TimeTracker.Repositories;
using TimeTracker.Services;
using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Transport;
using GraphQLParser;
using Newtonsoft.Json.Linq;
using TimeTracker.Models;
using WebSocketGraphql.Services.AuthenticationServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IChat, Chat>();
builder.Services.AddSingleton<IAuthorizationManager, AuthorizationManager>();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<IAuthorizationRepository, AuthorizationRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<ISchema, ChatSchema>(service =>
{
    return new ChatSchema(new SelfActivatingServiceProvider(service));
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
    .AddWebSocketAuthentication<CustomWebSocketAuthenticator>()
    .AddGraphTypes(typeof(ChatSchema).Assembly)
    .AddGraphTypes(typeof(IdentitySchema).Assembly);
});

var app = builder.Build();

app.UseCors(conf =>
{
    conf.WithOrigins(app.Configuration["FrontUrl"])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.UseGraphQL<ChatSchema>("/graphql", config =>
{
    config.AuthorizationRequired = true;
});

app.UseGraphQL<IdentitySchema>("/graphql-auth");

app.UseGraphQLAltair();

app.Run();