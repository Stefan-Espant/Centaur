using Centaur.Domain.Entities;

namespace Centaur.Application.Interfaces;

public interface IApiKeyRepository
{
    Task<IEnumerable<ApiKey>> GetByTenantIdAsync(Guid tenantId);
    Task<ApiKey> CreateAsync(ApiKey apiKey);
    Task DeleteAsync(Guid id);
}
