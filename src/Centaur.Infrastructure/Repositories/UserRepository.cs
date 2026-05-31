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

    public async Task<User> CreateAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}
