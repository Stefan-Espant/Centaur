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

    private static WebsiteSettingsDto DefaultSettings() =>
        new(
            "Mijn website",
            string.Empty,
            "Welkom op onze website",
            "Vertel hier kort wat bezoekers direct moeten weten.",
            string.Empty,
            string.Empty,
            "#1a1a1a");

    private static WebsiteSettingsDto Normalize(WebsiteSettingsDto settings) =>
        settings with
        {
            SiteName = settings.SiteName.Trim(),
            MetaDescription = settings.MetaDescription.Trim(),
            HeroTitle = settings.HeroTitle.Trim(),
            HeroSubtitle = settings.HeroSubtitle.Trim(),
            IntroText = settings.IntroText.Trim(),
            ContactEmail = settings.ContactEmail.Trim(),
            PrimaryColor = string.IsNullOrWhiteSpace(settings.PrimaryColor) ? "#1a1a1a" : settings.PrimaryColor.Trim()
        };
}
