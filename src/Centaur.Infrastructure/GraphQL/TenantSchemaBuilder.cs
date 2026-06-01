using System.Collections.Concurrent;
using System.Text;
using Centaur.Application.Interfaces;
using Centaur.Domain.Enums;

namespace Centaur.Infrastructure.GraphQL;

/// <summary>
/// Generates and caches GraphQL SDL per tenant, based on configured block types.
/// </summary>
public class TenantSchemaBuilder(IBlockTypeRepository blockTypeRepository)
{
    private static readonly ConcurrentDictionary<string, string> _schemaCache = new();

    public void InvalidateCache(string tenantSlug) =>
        _schemaCache.TryRemove(tenantSlug, out _);

    public async Task<string> GetSdlAsync(string tenantSlug)
    {
        if (_schemaCache.TryGetValue(tenantSlug, out var cached))
            return cached;

        var sdl = await BuildSdlAsync();
        _schemaCache[tenantSlug] = sdl;
        return sdl;
    }

    private async Task<string> BuildSdlAsync()
    {
        var blockTypes = await blockTypeRepository.GetAllAsync();
        var sb = new StringBuilder();

        var typeNames = blockTypes.Select(bt => ToPascalCase(bt.Slug) + "Block").ToList();

        foreach (var bt in blockTypes)
        {
            var typeName = ToPascalCase(bt.Slug) + "Block";
            sb.AppendLine($"type {typeName} {{");
            sb.AppendLine("  _type: String!");
            sb.AppendLine("  _id: ID!");

            foreach (var field in bt.Fields)
            {
                var fieldName = ToCamelCase(field.Slug);
                var gqlType = field.Type switch
                {
                    FieldType.Number => "Float",
                    FieldType.Boolean => "Boolean",
                    FieldType.Repeater => $"[{ToPascalCase(bt.Slug)}{ToPascalCase(field.Slug)}Row]",
                    _ => "String"
                };
                sb.AppendLine($"  {fieldName}: {gqlType}");
            }

            sb.AppendLine("}");
            sb.AppendLine();

            foreach (var field in bt.Fields.Where(f => f.Type == FieldType.Repeater))
            {
                if (!field.Config.TryGetValue("sub_fields", out var subFieldsEl))
                    continue;

                var subFields = System.Text.Json.JsonSerializer
                    .Deserialize<List<System.Text.Json.JsonElement>>(subFieldsEl) ?? [];

                var rowTypeName = $"{ToPascalCase(bt.Slug)}{ToPascalCase(field.Slug)}Row";
                sb.AppendLine($"type {rowTypeName} {{");
                foreach (var sf in subFields)
                {
                    if (!sf.TryGetProperty("slug", out var slugEl) || !sf.TryGetProperty("type", out var typeEl))
                        continue;
                    var sfName = ToCamelCase(slugEl.GetString() ?? "field");
                    var sfType = typeEl.GetString() switch
                    {
                        "number" => "Float",
                        "boolean" => "Boolean",
                        _ => "String"
                    };
                    sb.AppendLine($"  {sfName}: {sfType}");
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string ToPascalCase(string slug) =>
        string.Concat(slug.Split('_').Select(w => w.Length > 0
            ? char.ToUpper(w[0]) + w[1..] : w));

    private static string ToCamelCase(string slug)
    {
        var pascal = ToPascalCase(slug);
        return pascal.Length > 0 ? char.ToLower(pascal[0]) + pascal[1..] : pascal;
    }
}
