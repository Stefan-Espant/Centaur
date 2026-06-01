namespace Centaur.Application.DTOs;

public record CreateTenantRequest(string Name, string Slug, string AdminEmail, string AdminPassword);
