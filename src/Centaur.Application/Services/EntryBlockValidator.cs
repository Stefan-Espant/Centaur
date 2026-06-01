using System.Text.Json;
using Centaur.Application.Interfaces;
using Centaur.Domain.Enums;
using Centaur.Domain.ValueObjects;

namespace Centaur.Application.Services;

public class EntryBlockValidator(IBlockTypeRepository repository)
{
    public async Task<List<string>> ValidateAsync(
        List<FieldDefinition> fields,
        Dictionary<string, JsonElement> entryData)
    {
        var errors = new List<string>();

        foreach (var field in fields.Where(f => f.Type == FieldType.Blocks))
        {
            if (!entryData.TryGetValue(field.Slug, out var blockArrayEl))
                continue;

            if (blockArrayEl.ValueKind != JsonValueKind.Array)
            {
                errors.Add($"Veld '{field.Slug}' moet een array zijn.");
                continue;
            }

            var allowedSlugs = GetAllowedBlockTypeSlugs(field);

            foreach (var blockEl in blockArrayEl.EnumerateArray())
            {
                if (!blockEl.TryGetProperty("_type", out var typeEl))
                {
                    errors.Add($"Veld '{field.Slug}': elk blok moet een '_type' hebben.");
                    continue;
                }

                if (!blockEl.TryGetProperty("_id", out _))
                    errors.Add($"Veld '{field.Slug}': elk blok moet een '_id' hebben.");

                var blockTypeSlug = typeEl.GetString() ?? string.Empty;

                if (!allowedSlugs.Contains(blockTypeSlug))
                {
                    errors.Add($"Veld '{field.Slug}': bloktype '{blockTypeSlug}' is niet toegestaan in dit veld.");
                    continue;
                }

                var blockType = await repository.GetBySlugAsync(blockTypeSlug);
                if (blockType is null)
                    errors.Add($"Veld '{field.Slug}': bloktype '{blockTypeSlug}' bestaat niet.");
            }
        }

        return errors;
    }

    private static HashSet<string> GetAllowedBlockTypeSlugs(FieldDefinition field)
    {
        if (!field.Config.TryGetValue("allowed_block_type_slugs", out var slugsEl))
            return [];

        return slugsEl.Deserialize<List<string>>()?.ToHashSet() ?? [];
    }
}
