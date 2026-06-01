using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IPageService
{
    Task<IReadOnlyList<PageDto>> GetAllAsync(Guid tenantId);
    Task<PageDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<PageDto> CreateAsync(Guid tenantId, CreatePageRequest request);
    Task<PageDto> UpdateAsync(Guid tenantId, Guid id, UpdatePageRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
}
