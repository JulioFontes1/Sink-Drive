namespace SinkDrive.Models;

public class GoogleAccount
{
    public int Id { get; set; }
    public required string GoogleId { get; set; }
    public required string Email { get; set; }
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string? RefreshToken { get; set; }
    
}