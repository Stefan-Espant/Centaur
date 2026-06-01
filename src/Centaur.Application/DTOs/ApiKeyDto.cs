namespace Centaur.Application.DTOs;

public record ApiKeyDto(Guid Id, string Label, Guid TenantId, DateTime? ExpiresAt, DateTime CreatedAt);

public record CreatedApiKeyDto(Guid Id, string Label, string Key, Guid TenantId, DateTime? ExpiresAt, DateTime CreatedAt);
