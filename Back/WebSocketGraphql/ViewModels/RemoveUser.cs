namespace WebSocketGraphql.ViewModels;

public class RemoveUser
{
    public int Id { get; set; }

    public string Password { get; set; } = null!;
}