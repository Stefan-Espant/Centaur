using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IWebsiteSettingsRepository
{
    Task<WebsiteSettingsDto?> GetAsync(string tenantSchema);
    Task<WebsiteSettingsDto> UpsertAsync(string tenantSchema, WebsiteSettingsDto settings);
}
