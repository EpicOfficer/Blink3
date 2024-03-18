using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Blink3.API.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blink3.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties()
        {
            RedirectUri = returnUrl
        }, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> LogOut()
    {
        await authenticationService.SignOutAsync(HttpContext,
            CookieAuthenticationDefaults.AuthenticationScheme,
            new AuthenticationProperties()
            {
                RedirectUri = "/"
            });

        return Ok(new {message = "Logged out"});
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(User.GetAuthStatusModel());
    }
}