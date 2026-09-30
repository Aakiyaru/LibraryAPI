namespace Library.Application.Dtos;

public class AuthResponse
{
    public string Token { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
    public DateTime ExpiresAt { get; set; }
}