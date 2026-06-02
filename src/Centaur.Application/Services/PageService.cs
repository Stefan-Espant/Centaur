using System.Text.Json;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Enums;
using Centaur.Domain.ValueObjects;

namespace Centaur.Application.Services;

public class PageService(
    ITenantRepository tenantRepository,
    IPageRepository pageRepository,
    IBlockTypeRepository blockTypeRepository,
    EntryBlockValidator entryBlockValidator,
    IPageDemoService pageDemoService,
    IBlockTypePresetService blockTypePresetService) : IPageService
{
    public async Task<IReadOnlyList<PageDto>> GetAllAsync(Guid tenantId)
    {
        var schema = await GetTenantSchemaAsync(tenantId);
        await pageDemoService.EnsureDemoPageAsync(schema);
        return await pageRepository.GetAllAsync(schema);
    }

    public async Task<PageDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        var schema = await GetTenantSchemaAsync(tenantId);
        await pageDemoService.EnsureDemoPageAsync(schema);
        return await pageRepository.GetByIdAsync(schema, id);
    }

    public async Task<PageDto> CreateAsync(Guid tenantId, CreatePageRequest request)
    {
        var schema = await GetTenantSchemaAsync(tenantId);
        var normalized = Normalize(request.Title, request.Slug, request.MetaDescription, request.Body);
        await ValidateAsync(schema, normalized.Slug, normalized.Body);

        var now = DateTime.UtcNow;
        var page = new PageDto(
            Guid.NewGuid(),
            normalized.Title,
            normalized.Slug,
            normalized.MetaDescription,
            normalized.Body,
            now,
            now,
            "draft");

        return await pageRepository.CreateAsync(schema, page);
    }

    public async Task<PageDto> UpdateAsync(Guid tenantId, Guid id, UpdatePageRequest request)
    {
        var schema = await GetTenantSchemaAsync(tenantId);
        var existing = await pageRepository.GetByIdAsync(schema, id)
            ?? throw new KeyNotFoundException("Pagina niet gevonden.");

        var normalized = Normalize(request.Title, request.Slug, request.MetaDescription, request.Body);
        await ValidateAsync(schema, normalized.Slug, normalized.Body, id);

        var updated = existing with
        {
            Title = normalized.Title,
            Slug = normalized.Slug,
            MetaDescription = normalized.MetaDescription,
            Body = normalized.Body,
            UpdatedAt = DateTime.UtcNow,
            Status = NormalizeStatus(request.Status, existing.Status)
        };

        return await pageRepository.UpdateAsync(schema, updated);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        var schema = await GetTenantSchemaAsync(tenantId);
        await pageRepository.DeleteAsync(schema, id);
    }

    private async Task ValidateAsync(string tenantSchema, string slug, JsonElement body, Guid? pageId = null)
    {
        await blockTypePresetService.EnsurePresetsAsync();

        if (string.IsNullOrWhiteSpace(slug))
            throw new InvalidOperationException("Slug is verplicht.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new InvalidOperationException("Slug mag alleen kleine letters, cijfers en koppeltekens bevatten.");

        var pages = await pageRepository.GetAllAsync(tenantSchema);
        if (pages.Any(p => p.Slug == slug && p.Id != pageId))
            throw new InvalidOperationException($"Een pagina met slug '{slug}' bestaat al.");

        var allowedBlockTypeSlugs = (await blockTypeRepository.GetAllAsync())
            .Select(bt => bt.Slug)
            .Distinct()
            .ToArray();

        var blocksField = new FieldDefinition
        {
            Name = "Body",
            Slug = "body",
            Type = FieldType.Blocks,
            Config = new Dictionary<string, JsonElement>
            {
                ["allowed_block_type_slugs"] = JsonSerializer.SerializeToElement(allowedBlockTypeSlugs)
            }
        };

        using var dataDocument = JsonDocument.Parse(GetBodyJson(body));
        var errors = await entryBlockValidator.ValidateAsync(
            [blocksField],
            new Dictionary<string, JsonElement>
            {
                ["body"] = dataDocument.RootElement.Clone()
            });
        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(" ", errors));
    }

    private static (string Title, string Slug, string MetaDescription, JsonElement Body) Normalize(
        string title,
        string slug,
        string metaDescription,
        JsonElement body)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("Titel is verplicht.");

        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var normalizedBody = body.ValueKind == JsonValueKind.Undefined
            ? JsonSerializer.SerializeToElement(Array.Empty<object>())
            : body.Clone();

        return (
            title.Trim(),
            normalizedSlug,
            metaDescription.Trim(),
            normalizedBody
        );
    }

    private static string GetBodyJson(JsonElement body) =>
        body.ValueKind == JsonValueKind.Undefined ? "[]" : body.GetRawText();

    private static string NormalizeStatus(string? requested, string? existing) =>
        requested is "draft" or "published" or "archived" ? requested
        : existing is "draft" or "published" or "archived" ? existing
        : "published";

    private async Task<string> GetTenantSchemaAsync(Guid tenantId)
    {
        var tenant = await tenantRepository.GetByIdAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant niet gevonden.");

        return tenant.Slug.Replace("-", "_").ToLowerInvariant();
    }
}
