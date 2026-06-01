using System.Text.Json;

namespace Centaur.Application.DTOs;

public record CreatePageRequest(
    string Title,
    string Slug,
    string MetaDescription,
    JsonElement Body
);
