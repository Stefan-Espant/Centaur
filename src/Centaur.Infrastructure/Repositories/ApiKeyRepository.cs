using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class ApiKeyRepository(CentaurDbContext context) : IApiKeyRepository
{
    public async Task<IEnumerable<ApiKey>> GetByTenantIdAsync(Guid tenantId) =>
        await context.ApiKeys
            .Where(k => k.TenantId == tenantId)
            .OrderBy(k => k.Label)
            .ToListAsync();

    public async Task<ApiKey> CreateAsync(ApiKey apiKey)
    {
        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync();
        return apiKey;
    }

    public async Task DeleteAsync(Guid id)
    {
        var key = await context.ApiKeys.FindAsync(id);
        if (key is null) return;

        context.ApiKeys.Remove(key);
        await context.SaveChangesAsync();
    }
}
