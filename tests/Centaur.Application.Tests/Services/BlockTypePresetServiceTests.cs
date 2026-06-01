using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;

namespace Centaur.Application.Tests.Services;

public class BlockTypePresetServiceTests
{
    [Fact]
    public async Task EnsurePresetsAsync_CreatesExpectedPresetBlocks()
    {
        var repository = new MockBlockTypeRepository();
        var service = new BlockTypePresetService(repository);

        await service.EnsurePresetsAsync();

        var blockTypes = await repository.GetAllAsync();
        Assert.Contains(blockTypes, blockType => blockType.Slug == "section");
        Assert.Contains(blockTypes, blockType => blockType.Slug == "paragraph");
        Assert.Contains(blockTypes, blockType => blockType.Slug == "form");
        Assert.Contains(blockTypes, blockType => blockType.Slug == "reviews_block");
        Assert.Contains(blockTypes, blockType => blockType.Slug == "review");
    }
}
