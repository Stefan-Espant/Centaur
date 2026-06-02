using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;

namespace Centaur.Application.Services;

public class WebsiteSettingsService(
    ITenantRepository tenantRepository,
    IWebsiteSettingsRepository settingsRepository) : IWebsiteSettingsService
{
    public async Task<WebsiteSettingsDto> GetAsync(Guid tenantId)
    {
        var schema = await GetTenantSchemaAsync(tenantId);
        return await settingsRepository.GetAsync(schema) ?? DefaultSettings();
    }

    public async Task<WebsiteSettingsDto> UpdateAsync(Guid tenantId, WebsiteSettingsDto settings)
    {
        var schema = await GetTenantSchemaAsync(tenantId);
        return await settingsRepository.UpsertAsync(schema, Normalize(settings));
    }

    private async Task<string> GetTenantSchemaAsync(Guid tenantId)
    {
        var tenant = await tenantRepository.GetByIdAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant niet gevonden.");

        return tenant.Slug.Replace("-", "_").ToLowerInvariant();
    }

    private static WebsiteSettingsDto DefaultSettings() => new(
        SiteName: "Mijn website",
        Tagline: string.Empty,
        ContactEmail: string.Empty,
        Phone: string.Empty,
        MetaDescription: string.Empty,
        TitleSuffix: string.Empty,
        PrimaryColor: "#16a34a",
        SecondaryColor: string.Empty,
        Instagram: string.Empty,
        LinkedIn: string.Empty,
        Facebook: string.Empty,
        Twitter: string.Empty,
        AnalyticsId: string.Empty,
        CookieBannerEnabled: false,
        CookieBannerText: "Deze website gebruikt cookies voor een betere ervaring.",
        MaintenanceMode: false,
        MaintenanceMessage: "We zijn even bezig. Kom snel terug!",
        SchemaType: "Organization"
    );

    private static WebsiteSettingsDto Normalize(WebsiteSettingsDto s) => s with
    {
        SiteName       = s.SiteName.Trim(),
        Tagline        = s.Tagline.Trim(),
        ContactEmail   = s.ContactEmail.Trim(),
        Phone          = s.Phone.Trim(),
        MetaDescription = s.MetaDescription.Trim(),
        TitleSuffix    = s.TitleSuffix.Trim(),
        PrimaryColor   = string.IsNullOrWhiteSpace(s.PrimaryColor) ? "#16a34a" : s.PrimaryColor.Trim(),
        SecondaryColor = s.SecondaryColor.Trim(),
        Instagram      = s.Instagram.Trim(),
        LinkedIn       = s.LinkedIn.Trim(),
        Facebook       = s.Facebook.Trim(),
        Twitter        = s.Twitter.Trim(),
        AnalyticsId      = s.AnalyticsId.Trim(),
        CookieBannerText = s.CookieBannerText.Trim(),
        MaintenanceMessage = s.MaintenanceMessage.Trim(),
        SchemaType       = string.IsNullOrWhiteSpace(s.SchemaType) ? "Organization" : s.SchemaType.Trim()
    };
}
