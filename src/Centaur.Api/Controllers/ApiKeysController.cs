using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ApiKeysController(IApiKeyService apiKeyService) : ControllerBase
{
    private string? CurrentRole => User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

    private Guid? CurrentTenantId => Guid.TryParse(
        User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value, out var id)
            ? id
            : null;

    private bool IsSuperAdmin() => CurrentRole == UserRole.SuperAdmin.ToString();

    [HttpGet("tenants/{tenantId:guid}/api-keys")]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        if (!IsSuperAdmin()) return Forbid();
        return Ok(await apiKeyService.GetByTenantIdAsync(tenantId));
    }

    [HttpPost("tenants/{tenantId:guid}/api-keys")]
    public async Task<IActionResult> CreateForTenant(Guid tenantId, [FromBody] CreateApiKeyRequest request)
    {
        if (!IsSuperAdmin()) return Forbid();
        var key = await apiKeyService.CreateAsync(tenantId, request);
        return Created($"/api/tenants/{tenantId}/api-keys/{key.Id}", key);
    }

    [HttpDelete("tenants/{tenantId:guid}/api-keys/{keyId:guid}")]
    public async Task<IActionResult> DeleteFromTenant(Guid tenantId, Guid keyId)
    {
        if (!IsSuperAdmin()) return Forbid();
        await apiKeyService.DeleteAsync(keyId);
        return NoContent();
    }

    [HttpGet("api-keys")]
    public async Task<IActionResult> GetOwn()
    {
        if (CurrentTenantId is null) return Forbid();
        return Ok(await apiKeyService.GetByTenantIdAsync(CurrentTenantId.Value));
    }

    [HttpPost("api-keys")]
    public async Task<IActionResult> CreateOwn([FromBody] CreateApiKeyRequest request)
    {
        if (CurrentTenantId is null || IsSuperAdmin()) return Forbid();
        var key = await apiKeyService.CreateAsync(CurrentTenantId.Value, request);
        return Created($"/api/api-keys/{key.Id}", key);
    }

    [HttpDelete("api-keys/{keyId:guid}")]
    public async Task<IActionResult> DeleteOwn(Guid keyId)
    {
        if (CurrentTenantId is null || IsSuperAdmin()) return Forbid();
        await apiKeyService.DeleteAsync(keyId);
        return NoContent();
    }
}
