using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface ITenantService
{
    Task<TenantDto> CreateAsync(CreateTenantRequest request);
    Task<IEnumerable<TenantDto>> GetAllAsync();
    Task DeleteAsync(Guid id);
}
