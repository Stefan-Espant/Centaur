using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Infrastructure.GraphQL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/admin/block-types")]
[Authorize]
public class BlockTypesController(BlockTypeService blockTypeService, TenantSchemaBuilder schemaBuilder) : ControllerBase
{
    private string TenantSlug => User.FindFirst("tenant_slug")?.Value ?? string.Empty;
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await blockTypeService.GetAllAsync());

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await blockTypeService.GetBySlugAsync(slug);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateBlockTypeRequest request)
    {
        try
        {
            var created = await blockTypeService.CreateAsync(request);
            schemaBuilder.InvalidateCache(TenantSlug);
            return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = ex.Message.Split(". ").Where(e => !string.IsNullOrEmpty(e)) });
        }
    }

    [HttpPut("{slug}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateBlockTypeRequest request)
    {
        try
        {
            var updated = await blockTypeService.UpdateAsync(slug, request);
            schemaBuilder.InvalidateCache(TenantSlug);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = ex.Message.Split(". ").Where(e => !string.IsNullOrEmpty(e)) });
        }
    }

    [HttpDelete("{slug}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(string slug)
    {
        try
        {
            await blockTypeService.DeleteAsync(slug);
            schemaBuilder.InvalidateCache(TenantSlug);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
