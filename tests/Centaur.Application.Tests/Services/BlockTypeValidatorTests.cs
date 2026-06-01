using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Application.Services;
using Centaur.Domain.Entities;
using Moq;

namespace Centaur.Application.Tests.Services;

public class BlockTypeValidatorTests
{
    private Mock<IBlockTypeRepository> _repo = new();

    [Fact]
    public async Task ValidateCreate_WithUniqueSlug_ReturnsNoErrors()
    {
        _repo.Setup(r => r.GetBySlugAsync("hero")).ReturnsAsync((BlockType?)null);
        var validator = new BlockTypeValidator(_repo.Object);

        var errors = await validator.ValidateCreateAsync(
            new CreateBlockTypeRequest("Hero", "hero", []));

        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateCreate_WithDuplicateSlug_ReturnsSlugError()
    {
        _repo.Setup(r => r.GetBySlugAsync("hero"))
             .ReturnsAsync(new BlockType { Slug = "hero" });
        var validator = new BlockTypeValidator(_repo.Object);

        var errors = await validator.ValidateCreateAsync(
            new CreateBlockTypeRequest("Hero", "hero", []));

        Assert.Contains(errors, e => e.Contains("slug") || e.Contains("hero"));
    }

    [Fact]
    public async Task ValidateCreate_WithEmptyName_ReturnsNameError()
    {
        _repo.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((BlockType?)null);
        var validator = new BlockTypeValidator(_repo.Object);

        var errors = await validator.ValidateCreateAsync(
            new CreateBlockTypeRequest("", "hero", []));

        Assert.Contains(errors, e => e.Contains("naam") || e.Contains("name") || e.Contains("Name"));
    }

    [Fact]
    public async Task ValidateCreate_WithInvalidSlugFormat_ReturnsSlugFormatError()
    {
        _repo.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((BlockType?)null);
        var validator = new BlockTypeValidator(_repo.Object);

        var errors = await validator.ValidateCreateAsync(
            new CreateBlockTypeRequest("Hero Blok", "Hero Blok", []));

        Assert.Contains(errors, e => e.Contains("slug") || e.Contains("lowercase"));
    }

    [Fact]
    public async Task ValidateCreate_WithRepeaterSubFieldContainingBlocks_ReturnsNestingError()
    {
        _repo.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((BlockType?)null);
        var validator = new BlockTypeValidator(_repo.Object);

        var repeaterWithBlocksSubField = new FieldDefinitionDto(
            Guid.NewGuid(), "Items", "items", "repeater",
            new() {
                ["sub_fields"] = System.Text.Json.JsonSerializer
                    .Deserialize<System.Text.Json.JsonElement>(
                        """[{"id":"abc","name":"Nested","slug":"nested","type":"blocks","config":{}}]""")
            });

        var errors = await validator.ValidateCreateAsync(
            new CreateBlockTypeRequest("Test", "test", [repeaterWithBlocksSubField]));

        Assert.Contains(errors, e => e.Contains("nesting") || e.Contains("blocks") || e.Contains("repeater"));
    }
}
