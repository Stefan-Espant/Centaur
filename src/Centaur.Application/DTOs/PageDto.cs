using System.Text.Json;

namespace Centaur.Application.DTOs;

public record PageDto(
    Guid Id,
    string Title,
    string Slug,
    string MetaDescription,
    JsonElement Body,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Status
);
