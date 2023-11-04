namespace WebSocketGraphql.Services
{
    public static class EmailSendHelper
    {
        public static string GetUniqueCode()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
