namespace Library.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; }         // сам токен (случайная строка)
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }  // null = активен

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, int daysValid = 7)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = GenerateSecureToken(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(daysValid)
        };
    }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

    public void Revoke()
    {
        if (RevokedAt != null)
            throw new InvalidOperationException("Токен уже отозван");
        RevokedAt = DateTime.UtcNow;
    }

    private static string GenerateSecureToken()
    {
        // 64 байта случайных данных → 88 символов Base64
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}