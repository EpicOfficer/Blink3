using Blink3.Core.Caching;
using Blink3.Core.Models;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

public class GuildsController(DiscordSocketClient discordSocketClient, ICachingService cachingService) : ApiControllerBase(discordSocketClient, cachingService)
{
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(DiscordPartialGuild[]))]
    public async Task<ActionResult<DiscordPartialGuild[]>> GetAllGuilds()
    {
        List<DiscordPartialGuild> guilds = await GetUserGuilds();

        return Ok(guilds);
    }
}