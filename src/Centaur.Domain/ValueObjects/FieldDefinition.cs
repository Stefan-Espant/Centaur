using System.Text.Json;
using Centaur.Domain.Enums;

namespace Centaur.Domain.ValueObjects;

public class FieldDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public FieldType Type { get; set; }
    public Dictionary<string, JsonElement> Config { get; set; } = new();
}
