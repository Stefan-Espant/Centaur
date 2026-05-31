// src/Centaur.Infrastructure/Repositories/TenantRepository.cs
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class TenantRepository(CentaurDbContext context, TenantSchemaHelper schemaHelper) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id) =>
        context.Tenants.FirstOrDefaultAsync(t => t.Id == id);

    public Task<Tenant?> GetBySlugAsync(string slug) =>
        context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);

    public async Task<IEnumerable<Tenant>> GetAllAsync() =>
        await context.Tenants.OrderBy(t => t.Name).ToListAsync();

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        await schemaHelper.CreateTenantSchemaAsync(tenant.Slug);
        await transaction.CommitAsync();
        return tenant;
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await context.Tenants.FindAsync(id);
        if (tenant is null) return;
        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();
    }

    public Task<bool> SlugExistsAsync(string slug) =>
        context.Tenants.AnyAsync(t => t.Slug == slug);
}
