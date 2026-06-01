namespace Centaur.Application.DTOs;

public record UpdateBlockTypeRequest(
    string Name,
    List<FieldDefinitionDto> Fields
);
