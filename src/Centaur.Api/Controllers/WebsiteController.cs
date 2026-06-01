using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/website")]
[Authorize]
public class WebsiteController(IWebsiteSettingsService websiteSettingsService) : ControllerBase
{
    private string? CurrentRole => User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

    private Guid? CurrentTenantId => Guid.TryParse(
        User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value, out var id)
            ? id
            : null;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (CurrentTenantId is null) return Forbid();
        return Ok(await websiteSettingsService.GetAsync(CurrentTenantId.Value));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] WebsiteSettingsDto request)
    {
        if (CurrentTenantId is null || !CanEditWebsite()) return Forbid();
        return Ok(await websiteSettingsService.UpdateAsync(CurrentTenantId.Value, request));
    }

    private bool CanEditWebsite() =>
        CurrentRole == UserRole.Admin.ToString() || CurrentRole == UserRole.Editor.ToString();
}
