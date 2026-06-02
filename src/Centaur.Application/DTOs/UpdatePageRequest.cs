using System.Text.Json;

namespace Centaur.Application.DTOs;

public record UpdatePageRequest(
    string Title,
    string Slug,
    string MetaDescription,
    JsonElement Body,
    string? Status
);
