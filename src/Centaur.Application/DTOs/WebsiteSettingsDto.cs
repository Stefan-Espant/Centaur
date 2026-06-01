namespace Centaur.Application.DTOs;

public record WebsiteSettingsDto(
    string SiteName,
    string MetaDescription,
    string HeroTitle,
    string HeroSubtitle,
    string IntroText,
    string ContactEmail,
    string PrimaryColor);
