using System.Security.Claims;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUsersService usersService) : ControllerBase
{
    private const string AuthCookieName = "tasty-cookie";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await usersService.RegisterAsync(request, ct);
        return result.IsSuccess 
            ? Ok("User successfully registered") 
            : BadRequest(new { error = result.Error } );
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var token = await usersService.LoginAsync(request, ct);

        if (token.IsSuccess)
        {
            Response.Cookies.Append(AuthCookieName, token.Value);
            return Ok("User successfully logged");
        }

        return Unauthorized(new { error = token.Error });
    }
    
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName);
        return Ok("Успешный выход из системы.");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        return Ok(new { id = userId, email } );
    }
}