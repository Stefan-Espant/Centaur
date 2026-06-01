using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;
using Xunit;

namespace Centaur.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly MockUserRepository _repo = new();
    private const string JwtSecret = "test-secret-key-that-is-long-enough-32chars";

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.nl",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("wachtwoord123"),
            Role = UserRole.Admin,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        _repo.Seed(user);

        var service = new AuthService(_repo, JwtSecret);
        var result = await service.LoginAsync(new LoginRequest("admin@test.nl", "wachtwoord123"));

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.nl",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("juistewachtwoord"),
            Role = UserRole.Admin,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        _repo.Seed(user);

        var service = new AuthService(_repo, JwtSecret);
        var result = await service.LoginAsync(new LoginRequest("admin@test.nl", "foutewachtwoord"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsNull()
    {
        var service = new AuthService(_repo, JwtSecret);
        var result = await service.LoginAsync(new LoginRequest("onbekend@test.nl", "wachtwoord"));

        Assert.Null(result);
    }
}
