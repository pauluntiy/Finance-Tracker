using System.IdentityModel.Tokens.Jwt;
using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Auth;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    IUsersService usersService,
    ICurrentUserIdAccessor currentUserIdAccessor,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    private const string AuthCookieName = "tasty-cookie";

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await usersService.RegisterAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await usersService.LoginAsync(request, ct);

        if (result.IsFailure)
            return result.Error.ToActionResult();

        Response.Cookies.Append(
            AuthCookieName,
            result.Value.Token,
            CreateAuthCookieOptions());

        return Ok(result.Value.User);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName, CreateCookieDeletionOptions());
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = currentUserIdAccessor.UserId;
        var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        return Ok(new UserResponse(userId, email));
    }

    private CookieOptions CreateAuthCookieOptions() => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes),
        Secure = Request.IsHttps,
        Path = "/"
    };

    private CookieOptions CreateCookieDeletionOptions() => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = Request.IsHttps,
        Path = "/"
    };
}
