namespace Centaur.Application.DTOs;

public record CreateBlockTypeRequest(
    string Name,
    string Slug,
    List<FieldDefinitionDto> Fields
);
