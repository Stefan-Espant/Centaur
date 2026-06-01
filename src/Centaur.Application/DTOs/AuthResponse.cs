namespace Centaur.Application.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken, string Role, Guid? TenantId);
