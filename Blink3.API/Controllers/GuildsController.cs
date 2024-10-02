using Blink3.API.Interfaces;
using Blink3.Core.Caching;
using Blink3.Core.Models;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

[SwaggerTag("Endpoints for getting information on discord guilds")]
public class GuildsController(DiscordSocketClient discordSocketClient, ICachingService cachingService, IEncryptionService encryptionService)
    : ApiControllerBase(discordSocketClient, cachingService, encryptionService)
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Returns all Discord guilds",
        Description = "Returns a list of Discord guilds the currently logged in user has access to manage.",
        OperationId = "Guilds.GetAll",
        Tags = ["Guilds"]
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(DiscordPartialGuild[]))]
    public async Task<ActionResult<DiscordPartialGuild[]>> GetAllGuilds()
    {
        List<DiscordPartialGuild> guilds = await GetUserGuilds();

        return Ok(guilds);
    }

    [HttpGet("{id}/categories")]
    [SwaggerOperation(
        Summary = "Returns all categories for a guild",
        Description = "Returns a list of all Discord category channels for a given guild ID",
        OperationId = "Guilds.GetCategories",
        Tags = ["Guilds"]
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(DiscordPartialChannel[]))]
    public async Task<ActionResult<IReadOnlyCollection<DiscordPartialChannel>>> GetCategories(ulong id)
    {
        ObjectResult? accessCheckResult = await CheckGuildAccessAsync(id);
        if (accessCheckResult is not null) return accessCheckResult;

        return DiscordBotClient.GetGuild(id).CategoryChannels
            .OrderBy(c => c.Position)
            .Select(c =>
                new DiscordPartialChannel
                {
                    Id = c.Id,
                    Name = c.Name
                })
            .ToList();
    }
    
    [HttpGet("{id}/channels")]
    [SwaggerOperation(
        Summary = "Returns all channels for a guild",
        Description = "Returns a list of all Discord channels for a given guild ID",
        OperationId = "Guilds.GetChannels",
        Tags = ["Guilds"]
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(DiscordPartialChannel[]))]
    public async Task<ActionResult<IReadOnlyCollection<DiscordPartialChannel>>> GetChannels(ulong id)
    {
        ObjectResult? accessCheckResult = await CheckGuildAccessAsync(id);
        if (accessCheckResult is not null) return accessCheckResult;

        return DiscordBotClient.GetGuild(id).TextChannels
            .OrderBy(c => c.Position)
            .Select(c =>
                new DiscordPartialChannel
                {
                    Id = c.Id,
                    Name = c.Name
                })
            .ToList();
    }
}