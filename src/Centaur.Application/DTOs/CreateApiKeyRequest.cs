namespace Centaur.Application.DTOs;

public record CreateApiKeyRequest(string Label, DateTime? ExpiresAt);
