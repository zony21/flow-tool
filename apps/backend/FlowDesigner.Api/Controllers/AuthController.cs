using System.Security.Claims;
using FlowDesigner.Application.DTOs.Auth;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AppDbContext dbContext, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("github/login-url")]
    public IActionResult GetLoginUrl()
    {
        return Ok(new { url = "/api/auth/github/callback?demo=true" });
    }

    [HttpGet("github/callback")]
    public async Task<ActionResult<CurrentUserDto>> Callback([FromQuery] bool demo = true, CancellationToken cancellationToken = default)
    {
        if (!demo)
        {
            return BadRequest(new { message = "Only demo authentication is enabled in this environment." });
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.GitHubId == "demo-owner", cancellationToken);
        if (user is null)
        {
            user = new User
            {
                UserId = Guid.NewGuid(),
                GitHubId = "demo-owner",
                UserName = "demo-owner",
                DisplayName = "Demo Owner",
                Email = "demo@example.com",
                CreatedAtUtc = DateTime.UtcNow,
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.DisplayName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new CurrentUserDto(user.UserId, user.UserName, user.DisplayName, user.Email));
    }

    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken = default)
    {
        var currentUser = await currentUserService.GetCurrentUserAsync(cancellationToken);
        return currentUser is null ? Unauthorized() : Ok(currentUser);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
}
