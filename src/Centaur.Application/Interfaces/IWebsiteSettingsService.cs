using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IWebsiteSettingsService
{
    Task<WebsiteSettingsDto> GetAsync(Guid tenantId);
    Task<WebsiteSettingsDto> UpdateAsync(Guid tenantId, WebsiteSettingsDto settings);
}
