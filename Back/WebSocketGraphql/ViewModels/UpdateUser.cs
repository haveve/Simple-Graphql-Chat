namespace WebSocketGraphql.ViewModels;

public class UpdateUser
{
    public int Id { get; set; }
    
    public string NickName { get; set; } = null!;
    
    public string NewPassword { get; set; } = null!;
    
    public string OldPassword { get; set; } = null!;
    
    public string Email { get; set; } = null!;

    public string Salt { get; set; } = string.Empty;
}