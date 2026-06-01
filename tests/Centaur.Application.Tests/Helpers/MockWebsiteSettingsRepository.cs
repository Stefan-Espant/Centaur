using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;

namespace Centaur.Application.Tests.Helpers;

public class MockWebsiteSettingsRepository : IWebsiteSettingsRepository
{
    private readonly Dictionary<string, WebsiteSettingsDto> _settingsBySchema = new();

    public Task<WebsiteSettingsDto?> GetAsync(string tenantSchema)
    {
        _settingsBySchema.TryGetValue(tenantSchema, out var settings);
        return Task.FromResult(settings);
    }

    public Task<WebsiteSettingsDto> UpsertAsync(string tenantSchema, WebsiteSettingsDto settings)
    {
        _settingsBySchema[tenantSchema] = settings;
        return Task.FromResult(settings);
    }
}
