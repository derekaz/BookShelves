namespace BookShelves.Maui.Models;

public class AccessTokenInfo
{
    public required string Email { get; set; }
    public required LoginResponse LoginResponse { get; set; }
    public required DateTime AccessTokenExpiration { get; set; }
}
