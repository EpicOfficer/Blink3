using AspNet.Security.OAuth.Discord;
using Blink3.Common.Extensions;
using Blink3.Common.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Authentication related actions")]
    public class AuthController(IAuthenticationService authenticationService) : ControllerBase
    {
         [HttpGet("login")]
         [SwaggerOperation(
             Summary = "Initiates login via Discord",
             Description = "Redirects the user to Discord for OAuth",
             OperationId = "Auth.Login",
             Tags = new[] { "Auth" }
         )]
         public IActionResult Login(string returnUrl = "/")
         {
             return Challenge(new AuthenticationProperties()
             {
                 RedirectUri = returnUrl
             }, DiscordAuthenticationDefaults.AuthenticationScheme);
         }

         [HttpGet("logout")]
         [SwaggerOperation(
             Summary = "Logs out the user",
             Description = "Logs out the user",
             OperationId = "Auth.Logout",
             Tags = new[] { "Auth" }
         )]
         [SwaggerResponse(200, "Logged out", typeof(object))]
         public async Task<IActionResult> LogOut()
         {
             await authenticationService.SignOutAsync(HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                new AuthenticationProperties()
                {
                    RedirectUri = "/"
                });

            return Ok(new { message = "Logged Out" });
         }

         [HttpGet("status")]
         [SwaggerOperation(
             Summary = "Get user authentication status",
             Description = "Returns the authentication status of the user",
             OperationId = "Auth.Status",
             Tags = new[] { "Auth" }
         )]
         [SwaggerResponse(200, "Returned user authentication status", typeof(AuthStatus))]
         public IActionResult Status()
         {
             return Ok(User.GetAuthStatusModel());
         }
     }
}