namespace WebSocketGraphql.Helpers
{
    public static class IConfigurationExtention
    {
        public static int GetIteration(this IConfiguration configuration)
        {
            try
            {
                return Convert.ToInt32(configuration["PasswordHashing:Iteration"]);
            }
            catch
            {
                return 0;
            }
        }
    }
}
