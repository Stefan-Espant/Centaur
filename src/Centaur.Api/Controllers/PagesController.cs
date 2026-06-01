using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/pages")]
[Authorize]
public class PagesController(IPageService pageService) : ControllerBase
{
    private string? CurrentRole => User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

    private Guid? CurrentTenantId => Guid.TryParse(
        User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value, out var id)
            ? id
            : null;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (CurrentTenantId is null) return Forbid();
        return Ok(await pageService.GetAllAsync(CurrentTenantId.Value));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (CurrentTenantId is null) return Forbid();
        var page = await pageService.GetByIdAsync(CurrentTenantId.Value, id);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePageRequest request)
    {
        if (CurrentTenantId is null || !CanEditPages()) return Forbid();
        var page = await pageService.CreateAsync(CurrentTenantId.Value, request);
        return CreatedAtAction(nameof(GetById), new { id = page.Id }, page);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePageRequest request)
    {
        if (CurrentTenantId is null || !CanEditPages()) return Forbid();
        return Ok(await pageService.UpdateAsync(CurrentTenantId.Value, id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (CurrentTenantId is null || !CanEditPages()) return Forbid();
        await pageService.DeleteAsync(CurrentTenantId.Value, id);
        return NoContent();
    }

    private bool CanEditPages() =>
        CurrentRole == UserRole.Admin.ToString() || CurrentRole == UserRole.Editor.ToString();
}
