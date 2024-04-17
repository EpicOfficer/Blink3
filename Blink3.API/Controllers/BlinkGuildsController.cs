using Blink3.Core.Caching;
using Blink3.Core.DTOs;
using Blink3.Core.Entities;
using Blink3.Core.Models;
using Blink3.Core.Repositories.Interfaces;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

/// <summary>
///     Controller for performing CRUD operations on BlinkGuild items.
/// </summary>
[SwaggerTag("All CRUD operations for BlinkGuild items")]
public class BlinkGuildsController(DiscordSocketClient discordSocketClient, ICachingService cachingService, IBlinkGuildRepository blinkGuildRepository) : ApiControllerBase(discordSocketClient, cachingService)
{
    /// <summary>
    ///     Retrieves all BlinkGuild items that are manageable by the logged in user.
    /// </summary>
    /// <returns>A list of BlinkGuild objects representing the guild configurations.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Returns all BlinkGuild items",
        Description = "Returns a list of all of the BlinkGuilds that are manageable by the logged in user",
        OperationId = "BlinkGuilds.GetAll",
        Tags = ["BlinkGuilds"]
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(IEnumerable<BlinkGuild>))]
    public async Task<ActionResult<IEnumerable<BlinkGuild>>> GetAllBlinkGuilds(CancellationToken cancellationToken)
    {
        List<DiscordPartialGuild> guilds = await GetUserGuilds();
        IReadOnlyCollection<BlinkGuild> blinkGuilds = await blinkGuildRepository.FindByIdsAsync(guilds.Select(g => g.Id).ToHashSet());
        return Ok(blinkGuilds);
    }

    /// <summary>
    ///     Retrieves a specific BlinkGuild item by its Id.
    /// </summary>
    /// <param name="id">The Id of the BlinkGuild item.</param>
    /// <returns>The BlinkGuild item with the specified Id.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Returns a specific BlinkGuild item",
        Description = "Returns a BlinkGuild item by Id",
        OperationId = "BlinkGuilds.GetBlinkGuild",
        Tags = ["BlinkGuilds"]
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BlinkGuild))]
    public async Task<ActionResult<UserTodo>> GetBlinkGuild(ulong id)
    {
        ObjectResult? accessCheckResult = await CheckGuildAccessAsync(id);
        if (accessCheckResult is not null) return accessCheckResult;
        
        BlinkGuild blinkGuild = await blinkGuildRepository.GetOrCreateByIdAsync(id);
        return Ok(blinkGuild);
    }

    /// <summary>
    ///     Updates the content of a specific BlinkGuild item.
    /// </summary>
    /// <param name="id">The ID of the BlinkGuild item to update.</param>
    /// <param name="blinkGuild">The updated BlinkGuild item data.</param>
    /// <returns>
    ///     No content.
    /// </returns>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Updates a specific BlinkGuild item",
        Description = "Updates the content of a specific BlinkGuild item",
        OperationId = "BlinkGuilds.Update",
        Tags = ["BlinkGuilds"]
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "No content")]
    public async Task<ActionResult> UpdateBlinkGuild(ulong id, [FromBody] BlinkGuild blinkGuild,
        CancellationToken cancellationToken)
    {
        ObjectResult? accessCheckResult = await CheckGuildAccessAsync(id);
        if (accessCheckResult is not null) return accessCheckResult;

        blinkGuild.Id = id;
        await blinkGuildRepository.UpdateAsync(blinkGuild, cancellationToken);
        
        return NoContent();
    }
}