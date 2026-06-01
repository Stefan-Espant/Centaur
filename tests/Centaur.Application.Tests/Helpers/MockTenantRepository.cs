using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;

namespace Centaur.Application.Tests.Helpers;

public class MockTenantRepository : ITenantRepository
{
    private readonly List<Tenant> _tenants = new();

    public void Seed(Tenant tenant) => _tenants.Add(tenant);

    public Task<Tenant?> GetByIdAsync(Guid id) =>
        Task.FromResult(_tenants.FirstOrDefault(t => t.Id == id));

    public Task<Tenant?> GetBySlugAsync(string slug) =>
        Task.FromResult(_tenants.FirstOrDefault(t => t.Slug == slug));

    public Task<IEnumerable<Tenant>> GetAllAsync() =>
        Task.FromResult<IEnumerable<Tenant>>(_tenants);

    public Task<Tenant> CreateAsync(Tenant tenant)
    {
        _tenants.Add(tenant);
        return Task.FromResult(tenant);
    }

    public Task DeleteAsync(Guid id)
    {
        _tenants.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    public Task<bool> SlugExistsAsync(string slug) =>
        Task.FromResult(_tenants.Any(t => t.Slug == slug));
}
