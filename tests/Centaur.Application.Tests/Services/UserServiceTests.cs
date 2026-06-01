using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;

namespace Centaur.Application.Tests.Services;

public class UserServiceTests
{
    private readonly MockUserRepository _userRepository = new();

    [Fact]
    public async Task CreateAsync_ValidRole_ReturnsUserDto()
    {
        var tenantId = Guid.NewGuid();
        var service = new UserService(_userRepository);

        var result = await service.CreateAsync(tenantId, new CreateUserRequest("editor@test.nl", "Wachtwoord1!", "Editor"));

        Assert.Equal("editor@test.nl", result.Email);
        Assert.Equal("Editor", result.Role);
        Assert.Equal(tenantId, result.TenantId);
    }

    [Fact]
    public async Task CreateAsync_SuperAdminRole_ThrowsArgumentException()
    {
        var service = new UserService(_userRepository);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(Guid.NewGuid(), new CreateUserRequest("admin@test.nl", "Wachtwoord1!", "SuperAdmin")));
    }
}
