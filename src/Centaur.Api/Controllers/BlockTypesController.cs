using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/admin/block-types")]
[Authorize]
public class BlockTypesController(BlockTypeService blockTypeService) : ControllerBase
{
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
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
