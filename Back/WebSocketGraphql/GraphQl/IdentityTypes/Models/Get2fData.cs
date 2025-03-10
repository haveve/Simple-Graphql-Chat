namespace WebSocketGraphql.GraphQl.IdentityTypes.Models
{
    public class Get2fData
    {
        public string QrUrl { get; set; } = null!;
        
        public string ManualEntry { get; set; } = null!;

        public string Key { get; set; } = null!;
    }
}