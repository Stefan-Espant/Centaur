using Centaur.Application.Interfaces;

namespace Centaur.Api;

public class TenantSchemaAccessor(IHttpContextAccessor httpContextAccessor) : ITenantSchemaAccessor
{
    public string? CurrentSchema => httpContextAccessor.HttpContext?.Items["TenantSchema"] as string;
}
