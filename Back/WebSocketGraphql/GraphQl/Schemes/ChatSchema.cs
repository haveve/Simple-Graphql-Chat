﻿using GraphQL;
using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes;
using WebSocketGraphql.GraphQl.Directives.Validation;

namespace WebSocketGraphql.GraphQl.Schemes
{
    public class ChatSchema : Schema
    {
        public ChatSchema(IServiceProvider service) : base(service)
        {
            Metadata.Add(AuthorizationExtensions.AUTHORIZE_KEY, false);
            Directives.Register(new LengthDirective(),
                                new EmailDirective(),
                                new NumberRangeDivective());


            Query = service.GetRequiredService<ChatQuery>();
            Mutation = service.GetRequiredService<ChatMutation>();
            Subscription = service.GetRequiredService<ChatSubscriptions>();
        }
    }
}
