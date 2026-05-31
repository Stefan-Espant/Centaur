using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;

namespace Centaur.Application.Services;

public class TenantService(ITenantRepository tenantRepository, IUserRepository userRepository)
{
    public async Task<TenantDto> CreateAsync(CreateTenantRequest request)
    {
        if (await tenantRepository.SlugExistsAsync(request.Slug))
            throw new InvalidOperationException($"Slug '{request.Slug}' is al in gebruik.");

        var tenant = await tenantRepository.CreateAsync(new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        });

        await userRepository.CreateAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = request.AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
            Role = UserRole.Admin,
            TenantId = tenant.Id,
            CreatedAt = DateTime.UtcNow
        });

        return new TenantDto(tenant.Id, tenant.Slug, tenant.Name, tenant.CreatedAt);
    }

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        var tenants = await tenantRepository.GetAllAsync();
        return tenants.Select(t => new TenantDto(t.Id, t.Slug, t.Name, t.CreatedAt));
    }

    public async Task DeleteAsync(Guid id) => await tenantRepository.DeleteAsync(id);
}
