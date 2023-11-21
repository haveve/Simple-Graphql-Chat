using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using Dapper;
using TimeTracker.Models;
using WebSocketGraphql.Services;
using WebSocketGraphql.Models;
using WebSocketGraphql.ViewModels;
using System;

namespace TimeTracker.Repositories
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
    