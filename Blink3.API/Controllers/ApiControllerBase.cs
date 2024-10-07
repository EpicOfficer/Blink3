using System.Net.Mime;
using Blink3.API.Interfaces;
using Blink3.Core.Caching;
using Blink3.Core.DiscordAuth.Extensions;
using Blink3.Core.Models;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

/// <summary>
///     Base class for API controllers.
/// </summary>
/// <remarks>
///     This class provides common functionality and properties that can be used by derived API controllers.
/// </remarks>
[Consumes(MediaTypeNames.Application.Json)]
[ProducesErrorResponseType(typeof(ProblemDetails))]
[SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(ProblemDetails))]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public abstract class ApiControllerBase(DiscordRestClient botClient,
    Func<DiscordRestClient> userClientFactory,
    ICachingService cachingService,
    IEncryptionService encryptionService) : ControllerBase
{
    private readonly DiscordRestClient _userClient = userClientFactory();
    
    /// <summary>
    ///     Represents an Unauthorized Access message.
    /// </summary>
    private const string UnauthorizedAccessMessage = "You are not authorised to view this item.";

    /// <summary>
    ///     The message displayed when an item could not be found.
    /// </summary>
    private const string NotFoundAccessMessage = "Could not find an item with that ID";

    /// <summary>
    ///     Represents the user ID of the logged-in user.
    /// </summary>
    protected ulong UserId => User.GetUserId();

    /// <summary>
    ///     Creates a ProblemDetails object with a 404 Not Found status code and a specific error message.
    /// </summary>
    /// <returns>
    ///     Returns an ObjectResult that represents a ProblemDetails object with a 404 Not Found status code and a
    ///     specific error message.
    /// </returns>
    private ObjectResult ProblemForMissingItem()
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            detail: NotFoundAccessMessage
        );
    }

    /// <summary>
    ///     Generates a problem response for unauthorized access.
    /// </summary>
    /// <returns>
    ///     An <see cref="ObjectResult" /> representing a problem response with the following attributes:
    ///     - StatusCode: Status code indicating unauthorized access (401 Unauthorized).
    ///     - Detail: A message indicating that the user is not authorized to view the item.
    /// </returns>
    private ObjectResult ProblemForUnauthorizedAccess()
    {
        return Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            detail: UnauthorizedAccessMessage
        );
    }

    /// <summary>
    ///     Checks access to a resource based on the provided user ID.
    /// </summary>
    /// <param name="userId">The user ID to check access against.</param>
    /// <returns>
    ///     An <see cref="ObjectResult" /> representing a problem response if any.
    ///     Returns <c>null</c> if there are no errors.
    /// </returns>
    protected ObjectResult? CheckAccess(ulong? userId)
    {
        if (userId is null) return ProblemForMissingItem();
        return userId != UserId ? ProblemForUnauthorizedAccess() : null;
    }

    private async Task AuthenticateUserClientAsync()
    {
        string? encryptedToken = await cachingService.GetAsync<string>($"token:{UserId}");
        string? iv = await cachingService.GetAsync<string>($"token:{UserId}:iv");
        if (encryptedToken is null || iv is null) return;

        string accessToken = encryptionService.Decrypt(encryptedToken, iv);
        await _userClient.LoginAsync(TokenType.Bearer, accessToken);
    }

    protected async Task<List<DiscordPartialGuild>> GetUserGuilds()
    {
        await AuthenticateUserClientAsync();
        
        List<DiscordPartialGuild> managedGuilds = await cachingService.GetOrAddAsync($"discord:guilds:{UserId}",
            async () =>
            {
                List<DiscordPartialGuild> manageable = [];
                
                IAsyncEnumerable<IReadOnlyCollection<RestUserGuild>> guilds = _userClient.GetGuildSummariesAsync();
                await foreach (IReadOnlyCollection<RestUserGuild> guildCollection in guilds)
                {
                    manageable.AddRange(guildCollection.Where(g => g.Permissions.ManageGuild).Select(g =>
                        new DiscordPartialGuild
                        {
                            Id = g.Id,
                            Name = g.Name,
                            IconUrl = g.IconUrl
                        }));
                }

                return manageable;
            }, TimeSpan.FromMinutes(5));

        List<ulong> discordGuildIds = await botClient.GetGuildSummariesAsync()
            .SelectMany(guildCollection => guildCollection.ToAsyncEnumerable())
            .Select(guild => guild.Id)
            .ToListAsync();
        return managedGuilds.Where(g => discordGuildIds.Contains(g.Id)).ToList();
    }
    
    /// <summary>
    ///     Checks if the user has access to the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to check access for.</param>
    /// <returns>
    /// Returns an <see cref="ObjectResult"/> representing a problem response if the user doesn't have access, or null if the user has access.
    /// </returns>
    protected async Task<ObjectResult?> CheckGuildAccessAsync(ulong guildId)
    {
        List<DiscordPartialGuild> guilds = await GetUserGuilds();
        return guilds.Any(g => g.Id == guildId) ? null : ProblemForUnauthorizedAccess();
    }
    
    ~ApiControllerBase()
    {
        _userClient.LogoutAsync().Wait();
        _userClient.Dispose();
    }
}