using System.Text.Json;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;
using Centaur.Domain.ValueObjects;

namespace Centaur.Application.Services;

public class BlockTypePresetService(IBlockTypeRepository repository) : IBlockTypePresetService
{
    public async Task EnsurePresetsAsync()
    {
        var existingSlugs = (await repository.GetAllAsync())
            .Select(blockType => blockType.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var preset in GetPresets().Where(preset => !existingSlugs.Contains(preset.Slug)))
        {
            await repository.CreateAsync(preset);
        }
    }

    public IReadOnlyList<BlockTypeSummaryDto> GetPresetOverview() =>
        GetPresets()
            .Select(blockType => new BlockTypeSummaryDto(
                blockType.Name,
                blockType.Slug,
                blockType.Fields.Select(field => field.Name).ToList()))
            .ToList();

    private static List<BlockType> GetPresets() =>
    [
        Create("Section", "section", [
            TextField("Naam", "name"),
            TextField("Anchor", "anchor"),
            TextField("Variant", "variant"),
            SelectField("Breedte", "width", ["full", "wide", "contained"]),
            NumberField("Padding boven", "padding_top"),
            NumberField("Padding onder", "padding_bottom")
        ]),
        Create("Titel", "titel", [
            TextField("Kicker", "kicker"),
            TextField("Titel", "title"),
            SelectField("Heading niveau", "level", ["h1", "h2", "h3", "h4"]),
            SelectField("Uitlijning", "align", ["left", "center", "right"])
        ]),
        Create("Paragraaf", "paragraph", [
            TextField("Titel", "title"),
            RichTextField("Tekst", "content"),
            SelectField("Breedte", "width", ["narrow", "default", "wide"])
        ]),
        Create("Button", "button", [
            TextField("Label", "label"),
            TextField("Link", "href"),
            SelectField("Stijl", "style", ["primary", "secondary", "ghost"]),
            BooleanField("Open in nieuw tabblad", "new_tab")
        ]),
        Create("Media", "medium", [
            TextField("Bron / URL", "src"),
            TextField("Alt tekst", "alt"),
            TextField("Caption", "caption"),
            SelectField("Verhouding", "ratio", ["auto", "16:9", "4:3", "1:1"])
        ]),
        Create("Form", "form", [
            TextField("Titel", "title"),
            RichTextField("Intro", "intro"),
            TextField("Submit label", "submit_label"),
            TextField("Ontvanger e-mail", "recipient_email"),
            RepeaterField("Velden", "fields", [
                TextFieldDto("Label", "label"),
                TextFieldDto("Naam", "name"),
                SelectFieldDto("Type", "type", ["text", "email", "tel", "textarea"]),
                TextFieldDto("Placeholder", "placeholder"),
                BooleanFieldDto("Verplicht", "required")
            ])
        ]),
        Create("Video", "video", [
            TextField("Titel", "title"),
            TextField("Video URL", "url"),
            TextField("Poster image", "poster"),
            BooleanField("Autoplay", "autoplay")
        ]),
        Create("Galerij", "gallery", [
            TextField("Titel", "title"),
            RepeaterField("Items", "items", [
                TextFieldDto("Afbeelding URL", "image_url"),
                TextFieldDto("Alt tekst", "alt"),
                TextFieldDto("Caption", "caption")
            ])
        ]),
        Create("Code", "code", [
            TextField("Titel", "title"),
            SelectField("Taal", "language", ["html", "css", "javascript", "json", "plaintext"]),
            RichTextField("Code", "snippet")
        ]),
        Create("Layout instellingen", "layout_setting", [
            SelectField("Achtergrond", "background", ["default", "muted", "dark", "accent"]),
            SelectField("Container", "container", ["contained", "wide", "full"]),
            SelectField("Verticale uitlijning", "vertical_align", ["top", "center", "bottom"]),
            NumberField("Kolommen", "columns")
        ]),
        Create("Separating block", "separating_block", [
            SelectField("Stijl", "style", ["line", "space", "accent"]),
            NumberField("Ruimte boven", "space_top"),
            NumberField("Ruimte onder", "space_bottom")
        ]),
        Create("Inputveld", "inputveld", [
            TextField("Label", "label"),
            TextField("Naam", "name"),
            SelectField("Type", "type", ["text", "email", "tel", "textarea"]),
            TextField("Placeholder", "placeholder"),
            BooleanField("Verplicht", "required")
        ]),
        Create("Reviews blok", "reviews_block", [
            TextField("Titel", "title"),
            RichTextField("Intro", "intro"),
            RepeaterField("Reviews", "reviews", [
                TextFieldDto("Naam", "name"),
                TextFieldDto("Rol", "role"),
                RichTextFieldDto("Review", "quote"),
                NumberFieldDto("Score", "rating")
            ])
        ]),
        Create("Review", "review", [
            TextField("Naam", "name"),
            TextField("Rol", "role"),
            RichTextField("Review", "quote"),
            NumberField("Score", "rating")
        ])
    ];

    private static BlockType Create(string name, string slug, List<FieldDefinition> fields) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Fields = fields,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static FieldDefinition TextField(string name, string slug) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        Type = FieldType.Text
    };

    private static FieldDefinition RichTextField(string name, string slug) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        Type = FieldType.Richtext
    };

    private static FieldDefinition NumberField(string name, string slug) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        Type = FieldType.Number
    };

    private static FieldDefinition BooleanField(string name, string slug) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        Type = FieldType.Boolean
    };

    private static FieldDefinition SelectField(string name, string slug, List<string> options) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        Type = FieldType.Select,
        Config = new Dictionary<string, JsonElement>
        {
            ["options"] = JsonSerializer.SerializeToElement(options)
        }
    };

    private static FieldDefinition RepeaterField(string name, string slug, List<FieldDefinitionDto> subFields) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        Type = FieldType.Repeater,
        Config = new Dictionary<string, JsonElement>
        {
            ["sub_fields"] = JsonSerializer.SerializeToElement(subFields)
        }
    };

    private static FieldDefinitionDto TextFieldDto(string name, string slug) =>
        new(Guid.NewGuid(), name, slug, "text", new Dictionary<string, JsonElement>());

    private static FieldDefinitionDto RichTextFieldDto(string name, string slug) =>
        new(Guid.NewGuid(), name, slug, "richtext", new Dictionary<string, JsonElement>());

    private static FieldDefinitionDto NumberFieldDto(string name, string slug) =>
        new(Guid.NewGuid(), name, slug, "number", new Dictionary<string, JsonElement>());

    private static FieldDefinitionDto BooleanFieldDto(string name, string slug) =>
        new(Guid.NewGuid(), name, slug, "boolean", new Dictionary<string, JsonElement>());

    private static FieldDefinitionDto SelectFieldDto(string name, string slug, List<string> options) =>
        new(
            Guid.NewGuid(),
            name,
            slug,
            "select",
            new Dictionary<string, JsonElement>
            {
                ["options"] = JsonSerializer.SerializeToElement(options)
            });
}
