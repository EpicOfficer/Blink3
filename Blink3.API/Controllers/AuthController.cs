using System.Net.Http.Headers;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;
using Blink3.API.Models;
using Blink3.Core.Configuration;
using Blink3.Core.DiscordAuth;
using Blink3.Core.DiscordAuth.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[SwaggerTag("Authentication related actions")]
public class AuthController(IAuthenticationService authenticationService,
    IHttpClientFactory httpClientFactory,
    IOptions<BlinkConfiguration> config) : ControllerBase
{
    /// <summary>
    ///     Represents the configuration settings for the application.
    /// </summary>
    private BlinkConfiguration Config => config.Value;
    
    [HttpGet("login")]
    [SwaggerOperation(
        Summary = "Initiates login via Discord",
        Description = "Redirects the user to Discord for OAuth",
        OperationId = "Auth.Login",
        Tags = ["Auth"]
    )]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = returnUrl
        }, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    [SwaggerOperation(
        Summary = "Logs out the user",
        Description = "Logs out the user",
        OperationId = "Auth.Logout",
        Tags = ["Auth"]
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Logged out")]
    public async Task<IActionResult> LogOut()
    {
        await authenticationService.SignOutAsync(HttpContext,
            CookieAuthenticationDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = "/"
            });

        return NoContent();
    }

    [HttpGet("status")]
    [SwaggerOperation(
        Summary = "Get user authentication status",
        Description = "Returns the authentication status of the user",
        OperationId = "Auth.Status",
        Tags = ["Auth"]
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returned user authentication status", typeof(AuthStatus))]
    public IActionResult Status()
    {
        return Ok(User.GetAuthStatusModel());
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] DiscordTokenRequest tokenRequest)
    {
        string clientId = Config.Discord.ClientId;
        string clientSecret = Config.Discord.ClientSecret;
        
        using HttpClient httpClient = httpClientFactory.CreateClient();
        FormUrlEncodedContent requestBody = new(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", tokenRequest.Code)
        });
        
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;

        try
        {
            response = await httpClient.PostAsync(DiscordAuthenticationDefaults.TokenEndpoint, requestBody);
        }
        catch (Exception ex)
        {
            return BadRequest("Request failed - " + ex.Message);
        }

        if (!response.IsSuccessStatusCode) return BadRequest();
        
        string jsonResponse = await response.Content.ReadAsStringAsync();
        Dictionary<string, string>? token = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse);
        if (token is null || !token.TryGetValue("access_token", out string? value))
        {
            return BadRequest("Could not obtain access token");
        }
        
        return Ok(new { access_token = value });
    }
}