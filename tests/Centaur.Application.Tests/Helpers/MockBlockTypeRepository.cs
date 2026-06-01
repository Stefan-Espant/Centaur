using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;

namespace Centaur.Application.Tests.Helpers;

public class MockBlockTypeRepository : IBlockTypeRepository
{
    private readonly List<BlockType> _blockTypes = [];

    public Task<List<BlockType>> GetAllAsync() =>
        Task.FromResult(_blockTypes.OrderBy(blockType => blockType.Name).ToList());

    public Task<BlockType?> GetBySlugAsync(string slug) =>
        Task.FromResult(_blockTypes.FirstOrDefault(blockType => blockType.Slug == slug));

    public Task<BlockType> CreateAsync(BlockType blockType)
    {
        _blockTypes.Add(blockType);
        return Task.FromResult(blockType);
    }

    public Task<BlockType> UpdateAsync(BlockType blockType)
    {
        var index = _blockTypes.FindIndex(existing => existing.Id == blockType.Id);
        if (index >= 0) _blockTypes[index] = blockType;
        return Task.FromResult(blockType);
    }

    public Task DeleteAsync(string slug)
    {
        _blockTypes.RemoveAll(blockType => blockType.Slug == slug);
        return Task.CompletedTask;
    }
}
