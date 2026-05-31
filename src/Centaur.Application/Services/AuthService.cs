using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Centaur.Application.Services;

public class AuthService(IUserRepository userRepository, string jwtSecret) : IAuthService
{
    private readonly string _jwtSecret = Encoding.UTF8.GetByteCount(jwtSecret) >= 32
        ? jwtSecret
        : throw new ArgumentException("JWT secret must be at least 32 bytes.", nameof(jwtSecret));

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var accessToken = GenerateToken(user, TimeSpan.FromMinutes(15), "access");
        var refreshToken = GenerateToken(user, TimeSpan.FromDays(7), "refresh");

        return new AuthResponse(accessToken, refreshToken, user.Role.ToString(), user.TenantId);
    }

    private string GenerateToken(Domain.Entities.User user, TimeSpan expiry, string tokenType)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var claims = new[]
        {
            new Claim("user_id", user.Id.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("tenant_id", user.TenantId?.ToString() ?? string.Empty),
            new Claim("token_type", tokenType)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
