using System.Security.Cryptography;
using System.Text;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;

namespace Centaur.Application.Services;

public class ApiKeyService(IApiKeyRepository repository) : IApiKeyService
{
    public async Task<IEnumerable<ApiKeyDto>> GetByTenantIdAsync(Guid tenantId)
    {
        var keys = await repository.GetByTenantIdAsync(tenantId);
        return keys.Select(k => new ApiKeyDto(k.Id, k.Label, k.TenantId, k.ExpiresAt, k.CreatedAt));
    }

    public async Task<CreatedApiKeyDto> CreateAsync(Guid tenantId, CreateApiKeyRequest request)
    {
        var rawKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var keyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

        var apiKey = await repository.CreateAsync(new ApiKey
        {
            Id = Guid.NewGuid(),
            Label = request.Label,
            KeyHash = keyHash,
            TenantId = tenantId,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        });

        return new CreatedApiKeyDto(apiKey.Id, apiKey.Label, rawKey, apiKey.TenantId, apiKey.ExpiresAt, apiKey.CreatedAt);
    }

    public Task DeleteAsync(Guid id) => repository.DeleteAsync(id);
}
