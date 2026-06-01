using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result is null) return Unauthorized(new { message = "Ongeldig e-mailadres of wachtwoord." });
        return Ok(result);
    }
}
