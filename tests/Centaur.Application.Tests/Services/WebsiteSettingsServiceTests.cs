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
        Assert.Equal("#16a34a", result.PrimaryColor);
    }

    [Fact]
    public async Task UpdateAsync_StoresSettingsForTenantSchema()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepository.Seed(new Tenant { Id = tenantId, Slug = "demo-tenant", Name = "Demo", CreatedAt = DateTime.UtcNow });
        var service = new WebsiteSettingsService(_tenantRepository, _settingsRepository);

        await service.UpdateAsync(tenantId, new WebsiteSettingsDto(
            SiteName: " Demo site ",
            Tagline: "",
            ContactEmail: "info@example.com",
            Phone: "",
            MetaDescription: "",
            TitleSuffix: "",
            PrimaryColor: "#008866",
            SecondaryColor: "",
            Instagram: "",
            LinkedIn: "",
            Facebook: "",
            Twitter: "",
            AnalyticsId: "",
            CookieBannerEnabled: false,
            CookieBannerText: "",
            MaintenanceMode: false,
            MaintenanceMessage: "",
            SchemaType: "Organization"));

        var result = await service.GetAsync(tenantId);
        Assert.Equal("Demo site", result.SiteName);
        Assert.Equal("#008866", result.PrimaryColor);
    }
}
