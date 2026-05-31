using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;
using Centaur.Domain.Entities;
using Xunit;

namespace Centaur.Application.Tests.Services;

public class TenantServiceTests
{
    private readonly MockTenantRepository _tenantRepo = new();
    private readonly MockUserRepository _userRepo = new();

    [Fact]
    public async Task CreateTenant_NewSlug_ReturnsTenantDto()
    {
        var service = new TenantService(_tenantRepo, _userRepo);
        var request = new CreateTenantRequest("Bakkerij De Molen", "de-molen", "admin@demolen.nl", "Wachtwoord1!");

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("de-molen", result.Slug);
        Assert.Equal("Bakkerij De Molen", result.Name);
    }

    [Fact]
    public async Task CreateTenant_DuplicateSlug_ThrowsException()
    {
        _tenantRepo.Seed(new Tenant { Id = Guid.NewGuid(), Slug = "de-molen", Name = "Bestaande bakkerij", CreatedAt = DateTime.UtcNow });
        var service = new TenantService(_tenantRepo, _userRepo);
        var request = new CreateTenantRequest("Nieuwe Bakkerij", "de-molen", "nieuw@bakkerij.nl", "Wachtwoord1!");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task GetAll_ReturnsMappedDtos()
    {
        _tenantRepo.Seed(new Tenant { Id = Guid.NewGuid(), Slug = "tenant-a", Name = "Tenant A", CreatedAt = DateTime.UtcNow });
        _tenantRepo.Seed(new Tenant { Id = Guid.NewGuid(), Slug = "tenant-b", Name = "Tenant B", CreatedAt = DateTime.UtcNow });
        var service = new TenantService(_tenantRepo, _userRepo);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count());
    }
}
