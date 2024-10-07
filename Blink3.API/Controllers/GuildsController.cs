using Blink3.API.Interfaces;
using Blink3.Core.Caching;
using Blink3.Core.Models;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

[SwaggerTag("Endpoints for getting information on discord guilds")]
public class GuildsController(DiscordRestClient botClient,
    Func<DiscordRestClient> userClientFactory,
    ICachingService cachingService,
    IEncryptionService encryptionService)
    : ApiControllerBase(botClient, userClientFactory, cachingService, encryptionService)
{
    private readonly DiscordRestClient _botClient = botClient;
    private readonly ICachingService _cachingService = cachingService;

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

        string cacheKey = $"guild_{id}_categories";
        IReadOnlyCollection<DiscordPartialChannel> categories = await _cachingService.GetOrAddAsync(cacheKey, async () =>
        {
            RestGuild? guild = await _botClient.GetGuildAsync(id);
            IReadOnlyCollection<RestCategoryChannel>? categories = await guild.GetCategoryChannelsAsync();
            return categories
                .OrderBy(c => c.Position)
                .Select(c =>
                    new DiscordPartialChannel
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                .ToList();
        }, TimeSpan.FromMinutes(5));
        
        return Ok(categories);
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

        string cacheKey = $"guild_{id}_channels";
        IReadOnlyCollection<DiscordPartialChannel> channels = await _cachingService.GetOrAddAsync(cacheKey, async () =>
        {
            RestGuild? guild = await _botClient.GetGuildAsync(id);
            IReadOnlyCollection<RestTextChannel>? channels = await guild.GetTextChannelsAsync();
            return channels
                .OrderBy(c => c.Position)
                .Select(c =>
                    new DiscordPartialChannel
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                .ToList();
        }, TimeSpan.FromMinutes(5));

        return Ok(channels);
    }
}