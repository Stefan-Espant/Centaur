using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    private string? CurrentRole => User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

    private Guid? CurrentTenantId => Guid.TryParse(
        User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value, out var id)
            ? id
            : null;

    private bool IsSuperAdmin() => CurrentRole == UserRole.SuperAdmin.ToString();

    [HttpGet("tenants/{tenantId:guid}/users")]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        if (!IsSuperAdmin()) return Forbid();
        return Ok(await userService.GetByTenantIdAsync(tenantId));
    }

    [HttpPost("tenants/{tenantId:guid}/users")]
    public async Task<IActionResult> CreateForTenant(Guid tenantId, [FromBody] CreateUserRequest request)
    {
        if (!IsSuperAdmin()) return Forbid();

        try
        {
            var user = await userService.CreateAsync(tenantId, request);
            return Created($"/api/tenants/{tenantId}/users/{user.Id}", user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("tenants/{tenantId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> DeleteFromTenant(Guid tenantId, Guid userId)
    {
        if (!IsSuperAdmin()) return Forbid();
        await userService.DeleteAsync(userId);
        return NoContent();
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetOwn()
    {
        if (CurrentTenantId is null) return Forbid();
        return Ok(await userService.GetByTenantIdAsync(CurrentTenantId.Value));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateOwn([FromBody] CreateUserRequest request)
    {
        if (CurrentTenantId is null || IsSuperAdmin()) return Forbid();

        try
        {
            var user = await userService.CreateAsync(CurrentTenantId.Value, request);
            return Created($"/api/users/{user.Id}", user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteOwn(Guid userId)
    {
        if (CurrentTenantId is null || IsSuperAdmin()) return Forbid();
        await userService.DeleteAsync(userId);
        return NoContent();
    }
}
