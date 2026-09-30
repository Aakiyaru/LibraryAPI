using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Library.Application.Dtos;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Library.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Проверяем, что пользователь с таким email не существует
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email.ToLowerInvariant());
        if (exists)
            throw new InvalidOperationException("Пользователь с таким email уже зарегистрирован");

        // Хешируем пароль через BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Создаём пользователя через фабричный метод
        var user = User.Create(request.FullName, request.Email, passwordHash, role: "User");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Сразу выдаём токен, чтобы клиенту не нужно было логиниться отдельно
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (user == null)
            throw new UnauthorizedAccessException("Неверный email или пароль");

        // Проверяем пароль
        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValid)
            throw new UnauthorizedAccessException("Неверный email или пароль");

        return GenerateAuthResponse(user);
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // Claims — это данные, которые будут храниться в токене
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }
}