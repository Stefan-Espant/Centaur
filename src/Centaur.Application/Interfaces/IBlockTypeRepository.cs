using Centaur.Domain.Entities;

namespace Centaur.Application.Interfaces;

public interface IBlockTypeRepository
{
    Task<List<BlockType>> GetAllAsync();
    Task<BlockType?> GetBySlugAsync(string slug);
    Task<BlockType> CreateAsync(BlockType blockType);
    Task<BlockType> UpdateAsync(BlockType blockType);
    Task DeleteAsync(string slug);
}
