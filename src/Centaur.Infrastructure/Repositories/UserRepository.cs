// src/Centaur.Infrastructure/Repositories/UserRepository.cs
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class UserRepository(CentaurDbContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public Task<User?> GetByIdAsync(Guid id) =>
        context.Users.FindAsync(id).AsTask();

    public async Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId) =>
        await context.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Email)
            .ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user is null) return;

        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}
