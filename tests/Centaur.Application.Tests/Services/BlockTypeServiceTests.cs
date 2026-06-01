using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Application.Services;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;
using Centaur.Domain.ValueObjects;
using Moq;

namespace Centaur.Application.Tests.Services;

public class BlockTypeServiceTests
{
    private readonly Mock<IBlockTypeRepository> _repo = new();
    private readonly Mock<BlockTypeValidator> _validatorMock;
    private readonly BlockTypeService _service;

    public BlockTypeServiceTests()
    {
        _repo.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((BlockType?)null);
        _validatorMock = new Mock<BlockTypeValidator>(_repo.Object);
        _validatorMock.Setup(v => v.ValidateCreateAsync(It.IsAny<CreateBlockTypeRequest>()))
                      .ReturnsAsync([]);
        _validatorMock.Setup(v => v.ValidateUpdate(It.IsAny<UpdateBlockTypeRequest>()))
                      .Returns([]);
        _service = new BlockTypeService(_repo.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new BlockType
            {
                Id = Guid.NewGuid(), Name = "Hero", Slug = "hero",
                Fields = [new FieldDefinition { Name = "Titel", Slug = "title", Type = FieldType.Text }]
            }
        ]);

        var result = await _service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("hero", result[0].Slug);
        Assert.Single(result[0].Fields);
    }

    [Fact]
    public async Task GetBySlugAsync_WithExistingSlug_ReturnsDto()
    {
        _repo.Setup(r => r.GetBySlugAsync("hero")).ReturnsAsync(
            new BlockType { Id = Guid.NewGuid(), Name = "Hero", Slug = "hero", Fields = [] });

        var result = await _service.GetBySlugAsync("hero");

        Assert.NotNull(result);
        Assert.Equal("hero", result!.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_WithMissingSlug_ReturnsNull()
    {
        _repo.Setup(r => r.GetBySlugAsync("missing")).ReturnsAsync((BlockType?)null);

        var result = await _service.GetBySlugAsync("missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedDto()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<BlockType>()))
             .ReturnsAsync((BlockType bt) => bt);

        var request = new CreateBlockTypeRequest("Hero", "hero", []);
        var result = await _service.CreateAsync(request);

        Assert.Equal("hero", result.Slug);
        Assert.Equal("Hero", result.Name);
    }

    [Fact]
    public async Task CreateAsync_WithValidationErrors_ThrowsException()
    {
        _validatorMock.Setup(v => v.ValidateCreateAsync(It.IsAny<CreateBlockTypeRequest>()))
                      .ReturnsAsync(["Slug bestaat al."]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(new CreateBlockTypeRequest("Hero", "hero", [])));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingSlug_CallsRepository()
    {
        _repo.Setup(r => r.GetBySlugAsync("hero"))
             .ReturnsAsync(new BlockType { Slug = "hero" });
        _repo.Setup(r => r.DeleteAsync("hero")).Returns(Task.CompletedTask);

        await _service.DeleteAsync("hero");

        _repo.Verify(r => r.DeleteAsync("hero"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithMissingSlug_ThrowsException()
    {
        _repo.Setup(r => r.GetBySlugAsync("missing")).ReturnsAsync((BlockType?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync("missing"));
    }
}
