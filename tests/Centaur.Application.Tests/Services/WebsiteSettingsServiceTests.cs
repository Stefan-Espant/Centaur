using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;
using Centaur.Domain.Entities;

namespace Centaur.Application.Tests.Services;

public class WebsiteSettingsServiceTests
{
    private readonly MockTenantRepository _tenantRepository = new();
    private readonly MockWebsiteSettingsRepository _settingsRepository = new();

    [Fact]
    public async Task GetAsync_NoSettings_ReturnsDefaults()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepository.Seed(new Tenant { Id = tenantId, Slug = "demo-tenant", Name = "Demo", CreatedAt = DateTime.UtcNow });
        var service = new WebsiteSettingsService(_tenantRepository, _settingsRepository);

        var result = await service.GetAsync(tenantId);

        Assert.Equal("Mijn website", result.SiteName);
        Assert.Equal("#1a1a1a", result.PrimaryColor);
    }

    [Fact]
    public async Task UpdateAsync_StoresSettingsForTenantSchema()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepository.Seed(new Tenant { Id = tenantId, Slug = "demo-tenant", Name = "Demo", CreatedAt = DateTime.UtcNow });
        var service = new WebsiteSettingsService(_tenantRepository, _settingsRepository);

        await service.UpdateAsync(tenantId, new WebsiteSettingsDto(
            " Demo site ",
            "Meta",
            "Hero",
            "Sub",
            "Intro",
            "info@example.com",
            "#008866"));

        var result = await service.GetAsync(tenantId);
        Assert.Equal("Demo site", result.SiteName);
        Assert.Equal("#008866", result.PrimaryColor);
    }
}
