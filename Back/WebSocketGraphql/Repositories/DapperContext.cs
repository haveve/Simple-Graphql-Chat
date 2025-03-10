using System.Data.SqlClient;
using System.Data;
using Dapper;
using WebSocketGraphql.Models;
using WebSocketGraphql.Services;
using WebSocketGraphql.ViewModels;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.Models;

namespace WebSocketGraphql.Repositories
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SQLConnection");

           Mapper.SetMapper(typeof(User));
           Mapper.SetMapper(typeof(ChatModel));
           Mapper.SetMapper(typeof(Message));
           Mapper.SetMapper(typeof(ChatResult));
           Mapper.SetMapper(typeof(ChatParticipant));
           Mapper.SetMapper(typeof(RefreshToken));
           SqlMapper.AddTypeHandler(new DateTimeHandler());

        }
        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

    }
    public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }

        public override DateTime Parse(object value)
        {
            return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        }
    }
}
    