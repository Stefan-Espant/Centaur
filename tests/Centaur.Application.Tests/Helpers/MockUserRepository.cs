using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;

namespace Centaur.Application.Tests.Helpers;

public class MockUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public void Seed(User user) => _users.Add(user);

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    public Task<User?> GetByIdAsync(Guid id) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId) =>
        Task.FromResult<IEnumerable<User>>(_users.Where(u => u.TenantId == tenantId).OrderBy(u => u.Email).ToList());

    public Task<User> CreateAsync(User user)
    {
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task DeleteAsync(Guid id)
    {
        _users.RemoveAll(u => u.Id == id);
        return Task.CompletedTask;
    }
}
