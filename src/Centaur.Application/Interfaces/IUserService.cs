using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetByTenantIdAsync(Guid tenantId);
    Task<UserDto> CreateAsync(Guid tenantId, CreateUserRequest request);
    Task DeleteAsync(Guid id);
}
