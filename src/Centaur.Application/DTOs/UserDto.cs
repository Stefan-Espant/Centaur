namespace Centaur.Application.DTOs;

public record UserDto(Guid Id, string Email, string Role, Guid? TenantId, DateTime CreatedAt);
