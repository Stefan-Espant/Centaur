using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize]
public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsSuperAdmin()) return Forbid();
        var tenants = await tenantService.GetAllAsync();
        return Ok(tenants);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        if (!IsSuperAdmin()) return Forbid();
        try
        {
            var tenant = await tenantService.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), tenant);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!IsSuperAdmin()) return Forbid();
        await tenantService.DeleteAsync(id);
        return NoContent();
    }

    private bool IsSuperAdmin() =>
        User.Claims.FirstOrDefault(c => c.Type == "role")?.Value == UserRole.SuperAdmin.ToString();
}
