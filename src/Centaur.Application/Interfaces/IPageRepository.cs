using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IPageRepository
{
    Task<IReadOnlyList<PageDto>> GetAllAsync(string tenantSchema);
    Task<PageDto?> GetByIdAsync(string tenantSchema, Guid id);
    Task<PageDto> CreateAsync(string tenantSchema, PageDto page);
    Task<PageDto> UpdateAsync(string tenantSchema, PageDto page);
    Task DeleteAsync(string tenantSchema, Guid id);
}
