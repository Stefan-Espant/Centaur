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
            JsonSerializer.SerializeToElement(Array.Empty<object>())));

        var stored = await service.GetAllAsync(tenantId);
        Assert.Equal("Home", result.Title);
        Assert.Single(stored);
        Assert.Equal("home", stored[0].Slug);
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
            JsonSerializer.SerializeToElement(Array.Empty<object>())));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(tenantId, new CreatePageRequest(
                "Over",
                "home",
                string.Empty,
                JsonSerializer.SerializeToElement(Array.Empty<object>()))));
    }
}
