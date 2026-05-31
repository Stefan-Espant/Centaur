// src/Centaur.Domain/Entities/ApiKey.cs
namespace Centaur.Domain.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public string KeyHash { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
