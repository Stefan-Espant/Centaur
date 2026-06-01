using System.Text.Json;
using Centaur.Application.Interfaces;
using Centaur.Application.Services;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;
using Centaur.Domain.ValueObjects;
using Moq;

namespace Centaur.Application.Tests.Services;

public class EntryBlockValidatorTests
{
    private readonly Mock<IBlockTypeRepository> _repo = new();

    private static FieldDefinition MakeBlocksField(string fieldSlug, params string[] allowedSlugs)
    {
        var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            $$$"""{"allowed_block_type_slugs":[{{{string.Join(",", allowedSlugs.Select(s => $"\"{s}\""))}}}]}""")!;
        return new FieldDefinition { Slug = fieldSlug, Type = FieldType.Blocks, Config = config };
    }

    [Fact]
    public async Task Validate_WithValidBlockType_ReturnsNoErrors()
    {
        _repo.Setup(r => r.GetBySlugAsync("hero"))
             .ReturnsAsync(new BlockType { Slug = "hero", Fields = [] });

        var validator = new EntryBlockValidator(_repo.Object);
        var blocksField = MakeBlocksField("body", "hero");
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            """{"body":[{"_type":"hero","_id":"abc-123","title":"Hello"}]}""")!;

        var errors = await validator.ValidateAsync([blocksField], data);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Validate_WithBlockTypeNotInAllowedList_ReturnsError()
    {
        var validator = new EntryBlockValidator(_repo.Object);
        var blocksField = MakeBlocksField("body", "hero");
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            """{"body":[{"_type":"cta","_id":"abc-123"}]}""")!;

        var errors = await validator.ValidateAsync([blocksField], data);

        Assert.Contains(errors, e => e.Contains("cta") || e.Contains("body"));
    }

    [Fact]
    public async Task Validate_WithNonExistentBlockType_ReturnsError()
    {
        _repo.Setup(r => r.GetBySlugAsync("hero")).ReturnsAsync((BlockType?)null);

        var validator = new EntryBlockValidator(_repo.Object);
        var blocksField = MakeBlocksField("body", "hero");
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            """{"body":[{"_type":"hero","_id":"abc-123"}]}""")!;

        var errors = await validator.ValidateAsync([blocksField], data);

        Assert.Contains(errors, e => e.Contains("hero"));
    }

    [Fact]
    public async Task Validate_WithBlockMissingId_ReturnsError()
    {
        _repo.Setup(r => r.GetBySlugAsync("hero"))
             .ReturnsAsync(new BlockType { Slug = "hero", Fields = [] });

        var validator = new EntryBlockValidator(_repo.Object);
        var blocksField = MakeBlocksField("body", "hero");
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            """{"body":[{"_type":"hero"}]}""")!;

        var errors = await validator.ValidateAsync([blocksField], data);

        Assert.Contains(errors, e => e.Contains("_id") || e.Contains("id"));
    }

    [Fact]
    public async Task Validate_WithEmptyBlocksArray_ReturnsNoErrors()
    {
        var validator = new EntryBlockValidator(_repo.Object);
        var blocksField = MakeBlocksField("body", "hero");
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            """{"body":[]}""")!;

        var errors = await validator.ValidateAsync([blocksField], data);

        Assert.Empty(errors);
    }
}
