using Blink3.Core.Caching;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

public class GuildsController(ICachingService cachingService) : ApiControllerBase(cachingService)
{
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(RestUserGuild[]))]
    public async Task<ActionResult<RestUserGuild[]>> GetAllGuilds()
    {
        await InitDiscordClientAsync();
        
        IAsyncEnumerable<IReadOnlyCollection<RestUserGuild>>? guilds = Client?.GetGuildSummariesAsync();
        List<RestUserGuild> managedGuilds = [];

        if (guilds is not null)
            await foreach (IReadOnlyCollection<RestUserGuild> guildCollection in guilds)
            {
                managedGuilds.AddRange(guildCollection.Where(g => g.Permissions.ManageGuild));
            }

        return Ok(managedGuilds);
    }
}