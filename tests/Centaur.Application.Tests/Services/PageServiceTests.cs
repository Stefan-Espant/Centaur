using System.Text.Json;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;
using Centaur.Domain.Entities;
using Moq;

namespace Centaur.Application.Tests.Services;

public class PageServiceTests
{
    private readonly MockTenantRepository _tenantRepository = new();
    private readonly MockPageRepository _pageRepository = new();
    private readonly Mock<Centaur.Application.Interfaces.IBlockTypeRepository> _blockTypeRepository = new();
    private readonly Mock<IPageDemoService> _pageDemoService = new();
    private readonly Mock<IBlockTypePresetService> _blockTypePresetService = new();

    [Fact]
    public async Task CreateAsync_WithValidRequest_StoresPage()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepository.Seed(new Tenant { Id = tenantId, Slug = "demo-tenant", Name = "Demo", CreatedAt = DateTime.UtcNow });
        _blockTypeRepository.Setup(repository => repository.GetAllAsync()).ReturnsAsync([]);
        _pageDemoService.Setup(service => service.EnsureDemoPageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _blockTypePresetService.Setup(service => service.EnsurePresetsAsync()).Returns(Task.CompletedTask);

        var service = new PageService(
            _tenantRepository,
            _pageRepository,
            _blockTypeRepository.Object,
            new EntryBlockValidator(_blockTypeRepository.Object),
            _pageDemoService.Object,
            _blockTypePresetService.Object);

        var result = await service.CreateAsync(tenantId, new CreatePageRequest(
            "Home",
            "home",
            "Welkom",
            JsonSerializer.SerializeToElement(Array.Empty<object>()),
            null));

        var stored = await service.GetAllAsync(tenantId);
        Assert.Equal("Home", result.Title);
        Assert.Single(stored);
        Assert.Equal("home", stored[0].Slug);
    }

    [Fact]
    public async Task CreateAsync_AlwaysSetsDraftStatus()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepository.Seed(new Tenant { Id = tenantId, Slug = "demo-tenant", Name = "Demo", CreatedAt = DateTime.UtcNow });
        _blockTypeRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _pageDemoService.Setup(s => s.EnsureDemoPageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _blockTypePresetService.Setup(s => s.EnsurePresetsAsync()).Returns(Task.CompletedTask);

        var service = new PageService(
            _tenantRepository, _pageRepository, _blockTypeRepository.Object,
            new EntryBlockValidator(_blockTypeRepository.Object),
            _pageDemoService.Object, _blockTypePresetService.Object);

        var result = await service.CreateAsync(tenantId, new CreatePageRequest(
            "Test", "test", "", JsonSerializer.SerializeToElement(Array.Empty<object>()), "published"));

        Assert.Equal("draft", result.Status);
    }

    [Fact]
    public async Task UpdateAsync_WithExplicitStatus_UpdatesStatus()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepository.Seed(new Tenant { Id = tenantId, Slug = "demo-tenant", Name = "Demo", CreatedAt = DateTime.UtcNow });
        _blockTypeRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _pageDemoService.Setup(s => s.EnsureDemoPageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _blockTypePresetService.Setup(s => s.EnsurePresetsAsync()).Returns(Task.CompletedTask);

        var service = new PageService(
            _tenantRepository, _pageRepository, _blockTypeRepository.Object,
            new EntryBlockValidator(_blockTypeRepository.Object),
            _pageDemoService.Object, _blockTypePresetService.Object);

        var created = await service.CreateAsync(tenantId, new CreatePageRequest(
            "Test", "test", "", JsonSerializer.SerializeToElement(Array.Empty<object>()), null));

        Assert.Equal("draft", created.Status);

        var updated = await service.UpdateAsync(tenantId, created.Id, new UpdatePageRequest(
            "Test", "test", "", JsonSerializer.SerializeToElement(Array.Empty<object>()), "published"));

        Assert.Equal("published", updated.Status);
    }

    [Fact]
    public async Task UpdateAsync_WithNullStatus_PreservesExistingStatus()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepository.Seed(new Tenant { Id = tenantId, Slug = "demo-tenant", Name = "Demo", CreatedAt = DateTime.UtcNow });
        _blockTypeRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _pageDemoService.Setup(s => s.EnsureDemoPageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _blockTypePresetService.Setup(s => s.EnsurePresetsAsync()).Returns(Task.CompletedTask);

        var service = new PageService(
            _tenantRepository, _pageRepository, _blockTypeRepository.Object,
            new EntryBlockValidator(_blockTypeRepository.Object),
            _pageDemoService.Object, _blockTypePresetService.Object);

        var created = await service.CreateAsync(tenantId, new CreatePageRequest(
            "Test", "test", "", JsonSerializer.SerializeToElement(Array.Empty<object>()), null));
        await service.UpdateAsync(tenantId, created.Id, new UpdatePageRequest(
            "Test", "test", "", JsonSerializer.SerializeToElement(Array.Empty<object>()), "published"));

        var updated = await service.UpdateAsync(tenantId, created.Id, new UpdatePageRequest(
            "Test", "test", "", JsonSerializer.SerializeToElement(Array.Empty<object>()), null));

        Assert.Equal("published", updated.Status);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSlug_ThrowsException()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepository.Seed(new Tenant { Id = tenantId, Slug = "demo-tenant", Name = "Demo", CreatedAt = DateTime.UtcNow });
        _blockTypeRepository.Setup(repository => repository.GetAllAsync()).ReturnsAsync([]);
        _pageDemoService.Setup(service => service.EnsureDemoPageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _blockTypePresetService.Setup(service => service.EnsurePresetsAsync()).Returns(Task.CompletedTask);

        var service = new PageService(
            _tenantRepository,
            _pageRepository,
            _blockTypeRepository.Object,
            new EntryBlockValidator(_blockTypeRepository.Object),
            _pageDemoService.Object,
            _blockTypePresetService.Object);

        await service.CreateAsync(tenantId, new CreatePageRequest(
            "Home",
            "home",
            string.Empty,
            JsonSerializer.SerializeToElement(Array.Empty<object>()),
            null));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(tenantId, new CreatePageRequest(
                "Over",
                "home",
                string.Empty,
                JsonSerializer.SerializeToElement(Array.Empty<object>()),
                null)));
    }
}
