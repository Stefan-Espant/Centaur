using System.Text.Json;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;
using Centaur.Domain.ValueObjects;

namespace Centaur.Application.Services;

public class BlockTypeService(
    IBlockTypeRepository repository,
    BlockTypeValidator validator,
    IBlockTypePresetService presetService)
{
    public async Task<List<BlockTypeDto>> GetAllAsync()
    {
        await presetService.EnsurePresetsAsync();
        var blockTypes = await repository.GetAllAsync();
        return blockTypes.Select(ToDto).ToList();
    }

    public async Task<BlockTypeDto?> GetBySlugAsync(string slug)
    {
        await presetService.EnsurePresetsAsync();
        var bt = await repository.GetBySlugAsync(slug);
        return bt is null ? null : ToDto(bt);
    }

    public async Task<BlockTypeDto> CreateAsync(CreateBlockTypeRequest request)
    {
        var errors = await validator.ValidateCreateAsync(request);
        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(" ", errors));

        var blockType = new BlockType
        {
            Name = request.Name,
            Slug = request.Slug,
            Fields = request.Fields.Select(ToFieldDefinition).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await repository.CreateAsync(blockType);
        return ToDto(created);
    }

    public async Task<BlockTypeDto> UpdateAsync(string slug, UpdateBlockTypeRequest request)
    {
        var errors = validator.ValidateUpdate(request);
        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(" ", errors));

        var existing = await repository.GetBySlugAsync(slug)
            ?? throw new KeyNotFoundException($"Bloktype '{slug}' niet gevonden.");

        existing.Name = request.Name;
        existing.Fields = request.Fields.Select(ToFieldDefinition).ToList();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(existing);
        return ToDto(updated);
    }

    public async Task DeleteAsync(string slug)
    {
        var existing = await repository.GetBySlugAsync(slug)
            ?? throw new KeyNotFoundException($"Bloktype '{slug}' niet gevonden.");
        await repository.DeleteAsync(existing.Slug);
    }

    private static BlockTypeDto ToDto(BlockType bt) => new(
        bt.Id, bt.Name, bt.Slug,
        bt.Fields.Select(f => new FieldDefinitionDto(
            f.Id, f.Name, f.Slug, f.Type.ToString().ToLower(), f.Config)).ToList(),
        bt.CreatedAt, bt.UpdatedAt);

    private static FieldDefinition ToFieldDefinition(FieldDefinitionDto dto) => new()
    {
        Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
        Name = dto.Name,
        Slug = dto.Slug,
        Type = Enum.Parse<FieldType>(dto.Type, ignoreCase: true),
        Config = dto.Config ?? new()
    };
}
