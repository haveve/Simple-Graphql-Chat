using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using Dapper;
using TimeTracker.Models;
using WebSocketGraphql.Services;
using WebSocketGraphql.Models;

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

        }
        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

    }
}
    