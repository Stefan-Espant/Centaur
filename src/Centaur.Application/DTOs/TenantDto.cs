namespace Centaur.Application.DTOs;

public record TenantDto(Guid Id, string Slug, string Name, DateTime CreatedAt);
