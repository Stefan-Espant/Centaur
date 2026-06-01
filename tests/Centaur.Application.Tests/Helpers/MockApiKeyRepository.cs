using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;

namespace Centaur.Application.Tests.Helpers;

public class MockApiKeyRepository : IApiKeyRepository
{
    private readonly List<ApiKey> _apiKeys = new();

    public void Seed(ApiKey apiKey) => _apiKeys.Add(apiKey);

    public Task<IEnumerable<ApiKey>> GetByTenantIdAsync(Guid tenantId) =>
        Task.FromResult<IEnumerable<ApiKey>>(_apiKeys.Where(k => k.TenantId == tenantId).OrderBy(k => k.Label).ToList());

    public Task<ApiKey> CreateAsync(ApiKey apiKey)
    {
        _apiKeys.Add(apiKey);
        return Task.FromResult(apiKey);
    }

    public Task DeleteAsync(Guid id)
    {
        _apiKeys.RemoveAll(k => k.Id == id);
        return Task.CompletedTask;
    }
}
