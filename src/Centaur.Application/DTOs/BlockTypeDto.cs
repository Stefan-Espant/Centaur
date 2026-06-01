namespace Centaur.Application.DTOs;

public record BlockTypeDto(
    Guid Id,
    string Name,
    string Slug,
    List<FieldDefinitionDto> Fields,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
