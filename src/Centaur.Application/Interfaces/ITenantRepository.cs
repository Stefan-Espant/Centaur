using Centaur.Domain.Entities;

namespace Centaur.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetBySlugAsync(string slug);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Tenant> CreateAsync(Tenant tenant);
    Task DeleteAsync(Guid id);
    Task<bool> SlugExistsAsync(string slug);
}
