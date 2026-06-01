using System.Text.Json;

namespace Centaur.Application.DTOs;

public record FieldDefinitionDto(
    Guid Id,
    string Name,
    string Slug,
    string Type,
    Dictionary<string, JsonElement>? Config
);
