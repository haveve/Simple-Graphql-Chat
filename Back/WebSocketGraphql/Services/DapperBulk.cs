using Dapper;
using System.Data;

namespace TimeTracker.Services
{
    public static class DapperBulk
    {
        public static int BulkInsert<T>(this IDbConnection connection, List<T> list, Func<T, string> toSpecificString, string values, string toTabel)
        {
            string query = $"INSERT INTO {toTabel} {values} VALUES";

            int count = 0;

            for (int i = 0; i < list.Count(); i++)
            {

                query += toSpecificString(list[i]) + ',';


                if ((i+1) % 1000 == 0)
                {
                    query = query.TrimEnd(',');
                    count += connection.Execute(query);
                    query = $"INSERT INTO {toTabel} {values} VALUES";
                }

            }

            query = query.TrimEnd(',');
            if(count%999 != 0 || count == 0)
            count += connection.Execute(query);

            return count;
        }
    }
}
