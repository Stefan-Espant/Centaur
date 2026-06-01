using Centaur.Domain.Entities;

namespace Centaur.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId);
    Task<User> CreateAsync(User user);
    Task DeleteAsync(Guid id);
}
