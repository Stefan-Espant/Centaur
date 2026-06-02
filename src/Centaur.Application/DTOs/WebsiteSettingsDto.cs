namespace Centaur.Application.DTOs;

public record WebsiteSettingsDto(
    // Algemeen
    string SiteName,
    string Tagline,
    string ContactEmail,
    string Phone,
    // SEO
    string MetaDescription,
    string TitleSuffix,
    // Huisstijl
    string PrimaryColor,
    string SecondaryColor,
    // Social media
    string Instagram,
    string LinkedIn,
    string Facebook,
    string Twitter,
    // Technisch
    string AnalyticsId,
    bool CookieBannerEnabled,
    string CookieBannerText,
    bool MaintenanceMode,
    string MaintenanceMessage,
    // SEO extra
    string SchemaType
);
