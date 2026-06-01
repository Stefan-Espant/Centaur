using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IApiKeyService
{
    Task<IEnumerable<ApiKeyDto>> GetByTenantIdAsync(Guid tenantId);
    Task<CreatedApiKeyDto> CreateAsync(Guid tenantId, CreateApiKeyRequest request);
    Task DeleteAsync(Guid id);
}
