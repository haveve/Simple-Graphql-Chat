using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Tests.Subscription;
using GraphQL.Types;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IChat, Chat>();

builder.Services.AddSingleton<ISchema, ChatSchema>(service =>
{
    return new ChatSchema(new SelfActivatingServiceProvider(service));
});

builder.Services.AddCors();

builder.Services.AddGraphQL(c =>
{
    c.AddSystemTextJson()
    .AddSchema<ChatSchema>()
    .AddGraphTypes(typeof(ChatSchema).Assembly);
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

app.UseWebSockets();

app.UseGraphQL<ChatSchema>();

app.UseGraphQLAltair();

app.Run();