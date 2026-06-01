using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;

namespace Centaur.Application.Tests.Helpers;

public class MockPageRepository : IPageRepository
{
    private readonly Dictionary<string, List<PageDto>> _pagesBySchema = new();

    public Task<IReadOnlyList<PageDto>> GetAllAsync(string tenantSchema)
    {
        if (!_pagesBySchema.TryGetValue(tenantSchema, out var pages))
            return Task.FromResult<IReadOnlyList<PageDto>>([]);

        return Task.FromResult<IReadOnlyList<PageDto>>(pages.OrderBy(page => page.Title).ToList());
    }

    public async Task<PageDto?> GetByIdAsync(string tenantSchema, Guid id) =>
        (await GetAllAsync(tenantSchema)).FirstOrDefault(page => page.Id == id);

    public Task<PageDto> CreateAsync(string tenantSchema, PageDto page)
    {
        var pages = GetOrCreate(tenantSchema);
        pages.Add(page);
        return Task.FromResult(page);
    }

    public Task<PageDto> UpdateAsync(string tenantSchema, PageDto page)
    {
        var pages = GetOrCreate(tenantSchema);
        var index = pages.FindIndex(existing => existing.Id == page.Id);
        if (index >= 0) pages[index] = page;
        return Task.FromResult(page);
    }

    public Task DeleteAsync(string tenantSchema, Guid id)
    {
        var pages = GetOrCreate(tenantSchema);
        pages.RemoveAll(page => page.Id == id);
        return Task.CompletedTask;
    }

    private List<PageDto> GetOrCreate(string tenantSchema)
    {
        if (!_pagesBySchema.TryGetValue(tenantSchema, out var pages))
        {
            pages = [];
            _pagesBySchema[tenantSchema] = pages;
        }

        return pages;
    }
}
