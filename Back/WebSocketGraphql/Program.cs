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

public static class AuthOptions
{
    public static SymmetricSecurityKey GetSymmetricSecurityKey(string KEY) =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));

    public static string GetKey(this IConfiguration configuration)
    {
        return configuration["Authorization:Key"];
    }

    public static string GetIssuer(this IConfiguration configuration)
    {
        return configuration["Authorization:Issuer"];
    }

    public static string GetAudience(this IConfiguration configuration)
    {
        return configuration["Authorization:Audience"];
    }
}

public class CustomJwtBearerHandler : JwtBearerHandler
{
    private readonly IConfiguration _configuration;
    private readonly IAuthorizationManager _authorizationManager;

    public CustomJwtBearerHandler(IAuthorizationManager authorizationManager, IConfiguration configuration, IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        if (!Context.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValues))
        {
            return AuthenticateResult.Fail("Authorization header not found.");
        }

        var authorizationHeader = authorizationHeaderValues.FirstOrDefault();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return AuthenticateResult.Fail("Bearer token not found in Authorization header.");
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();

        if (_authorizationManager.IsValidToken(token))
        {
            var tokenData = _authorizationManager.ReadJwtToken(token);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(tokenData.Claims, "Token"));
            return AuthenticateResult.Success(new AuthenticationTicket(principal, "CustomJwtBearer"));
        }

        return AuthenticateResult.Fail("Token validation failed.");
    }
}


class CustomWebSocketAuthenticator : IWebSocketAuthenticationService
{
    private readonly IGraphQLSerializer _serializer;
    private readonly IAuthorizationManager _authorizationManager;

    public CustomWebSocketAuthenticator(IGraphQLSerializer serializer,IAuthorizationManager authorizationManager)
    {
        _serializer = serializer;
        _authorizationManager = authorizationManager;
    }

    public Task AuthenticateAsync(IWebSocketConnection connection, string subProtocol, OperationMessage operationMessage)
    {
        var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);
        if ((payload?.TryGetValue("authorization", out var token) ?? false) && token is string tokeString)
        {
            if (_authorizationManager.IsValidToken(tokeString))
            {
                var tokenData = _authorizationManager.ReadJwtToken(tokeString);
                var principal = new ClaimsPrincipal(new ClaimsIdentity(tokenData.Claims, "Token"));
                connection.HttpContext.User = principal;
            }
        }
        return Task.CompletedTask;
    }
}