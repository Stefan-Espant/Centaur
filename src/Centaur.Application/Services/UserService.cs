using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;

namespace Centaur.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetByTenantIdAsync(Guid tenantId)
    {
        var users = await userRepository.GetByTenantIdAsync(tenantId);
        return users.Select(u => new UserDto(u.Id, u.Email, u.Role.ToString(), u.TenantId, u.CreatedAt));
    }

    public async Task<UserDto> CreateAsync(Guid tenantId, CreateUserRequest request)
    {
        if (!Enum.TryParse<UserRole>(request.Role, out var role) || role == UserRole.SuperAdmin)
            throw new ArgumentException($"Ongeldige rol: {request.Role}");

        var user = await userRepository.CreateAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(request.Password)),
            Role = role,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        });

        return new UserDto(user.Id, user.Email, user.Role.ToString(), user.TenantId, user.CreatedAt);
    }

    public Task DeleteAsync(Guid id) => userRepository.DeleteAsync(id);
}
