using System.Text.Json;
using System.Text.RegularExpressions;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;

namespace Centaur.Application.Services;

public class BlockTypeValidator(IBlockTypeRepository repository)
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9_]+$", RegexOptions.Compiled);

    public virtual async Task<List<string>> ValidateCreateAsync(CreateBlockTypeRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Name is verplicht.");

        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            errors.Add("Slug is verplicht.");
        }
        else if (!SlugRegex.IsMatch(request.Slug))
        {
            errors.Add("Slug mag alleen lowercase letters, cijfers en underscores bevatten.");
        }
        else
        {
            var existing = await repository.GetBySlugAsync(request.Slug);
            if (existing is not null)
                errors.Add($"Een bloktype met slug '{request.Slug}' bestaat al.");
        }

        errors.AddRange(ValidateFields(request.Fields));
        return errors;
    }

    public virtual List<string> ValidateUpdate(UpdateBlockTypeRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Name is verplicht.");
        errors.AddRange(ValidateFields(request.Fields));
        return errors;
    }

    private static IEnumerable<string> ValidateFields(List<FieldDefinitionDto> fields)
    {
        foreach (var field in fields)
        {
            if (field.Type == "repeater" && field.Config is not null
                && field.Config.TryGetValue("sub_fields", out var subFieldsEl))
            {
                var subFields = subFieldsEl.Deserialize<List<JsonElement>>() ?? [];
                foreach (var sf in subFields)
                {
                    if (sf.TryGetProperty("type", out var typeEl))
                    {
                        var type = typeEl.GetString();
                        if (type == "blocks" || type == "repeater")
                            yield return $"Veld '{field.Slug}': sub-velden van een repeater mogen geen type 'blocks' of 'repeater' hebben (geen nesting).";
                    }
                }
            }
        }
    }
}
