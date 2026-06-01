using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class BlockTypeRepository(CentaurDbContext context) : IBlockTypeRepository
{
    public async Task<List<BlockType>> GetAllAsync() =>
        await context.BlockTypes.OrderBy(bt => bt.Name).ToListAsync();

    public async Task<BlockType?> GetBySlugAsync(string slug) =>
        await context.BlockTypes.FirstOrDefaultAsync(bt => bt.Slug == slug);

    public async Task<BlockType> CreateAsync(BlockType blockType)
    {
        context.BlockTypes.Add(blockType);
        await context.SaveChangesAsync();
        return blockType;
    }

    public async Task<BlockType> UpdateAsync(BlockType blockType)
    {
        context.BlockTypes.Update(blockType);
        await context.SaveChangesAsync();
        return blockType;
    }

    public async Task DeleteAsync(string slug)
    {
        var blockType = await context.BlockTypes.FirstOrDefaultAsync(bt => bt.Slug == slug);
        if (blockType is null) return;
        context.BlockTypes.Remove(blockType);
        await context.SaveChangesAsync();
    }
}
