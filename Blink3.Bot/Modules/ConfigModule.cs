using Blink3.Core.Entities;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Color = SixLabors.ImageSharp.Color;

namespace Blink3.Bot.Modules;

[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[CommandContextType(InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.GuildInstall)]
[UsedImplicitly]
[Group("config", "Configure the bot for this server")]
public class ConfigModule(IUnitOfWork unitOfWork, ILogger<ConfigModule> logger)
    : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    public enum SettingsEnum
    {
        [ChoiceDisplay("Wordle Background Colour")]
        WordleBackgroundColour,

        [ChoiceDisplay("Wordle Text Colour")] WordleTextColour,

        [ChoiceDisplay("Wordle Correct Tile Colour")]
        WordleCorrectTileColour,

        [ChoiceDisplay("Wordle Misplaced Tile Colour")]
        WordleMisplacedTileColour,

        [ChoiceDisplay("Wordle Incorrect Tile Colour")]
        WordleIncorrectTileColour,

        [ChoiceDisplay("Staff Logging Channel")]
        StaffLoggingChannel,

        [ChoiceDisplay("Temporary VC Category")]
        TempVcCategory
    }

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    [SlashCommand("set", "Change or reset config values")]
    public async Task Set(SettingsEnum setting, string? value = null)
    {
        GuildLogContext guildLogContext = new(Context.Guild);
        UserLogContext userLogContext = new(Context.User);

        using (logger.BeginScope(new { Guild = guildLogContext, User = userLogContext }))
        {
            BlinkGuild guild = await FetchConfig();

            switch (setting)
            {
                case SettingsEnum.WordleBackgroundColour:
                    await SetPropertyColour(guild, value, guildLogContext, userLogContext,
                        (entity, colour) => entity.BackgroundColour = colour?.ToHex() ?? string.Empty);
                    break;
                case SettingsEnum.WordleTextColour:
                    await SetPropertyColour(guild, value, guildLogContext, userLogContext,
                        (entity, colour) => entity.TextColour = colour?.ToHex() ?? string.Empty);
                    break;
                case SettingsEnum.WordleCorrectTileColour:
                    await SetPropertyColour(guild, value, guildLogContext, userLogContext,
                        (entity, colour) => entity.CorrectTileColour = colour?.ToHex() ?? string.Empty);
                    break;
                case SettingsEnum.WordleMisplacedTileColour:
                    await SetPropertyColour(guild, value, guildLogContext, userLogContext,
                        (entity, colour) => entity.MisplacedTileColour = colour?.ToHex() ?? string.Empty);
                    break;
                case SettingsEnum.WordleIncorrectTileColour:
                    await SetPropertyColour(guild, value, guildLogContext, userLogContext,
                        (entity, colour) => entity.IncorrectTileColour = colour?.ToHex() ?? string.Empty);
                    break;
                case SettingsEnum.StaffLoggingChannel:
                    await SetPropertyUlong(guild, value, guildLogContext, userLogContext,
                        (entity, channel) => entity.LoggingChannelId = channel);
                    break;
                case SettingsEnum.TempVcCategory:
                    await SetPropertyUlong(guild, value, guildLogContext, userLogContext,
                        (entity, category) => entity.TemporaryVcCategoryId = category);
                    break;
                default:
                    await RespondErrorAsync("Unrecognised setting", "The setting you provided is not recognised.");
                    logger.LogWarning("Unrecognized setting {Setting} by {User} in {Guild}", setting, userLogContext,
                        guildLogContext);
                    break;
            }
        }
    }

    /// <summary>
    ///     Sets the value of a ulong property in the BlinkGuild object.
    /// </summary>
    /// <param name="guild">The BlinkGuild object to modify.</param>
    /// <param name="value">The new value of the ulong property as a string. Use null to reset it to default.</param>
    /// <param name="guildLogContext">The GuildLogContext associated with the guild where the operation is performed.</param>
    /// <param name="userLogContext">The UserLogContext representing the user performing the operation.</param>
    /// <param name="setUlong">The action to set the ulong property in the BlinkGuild object.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SetPropertyUlong(BlinkGuild guild,
        string? value,
        GuildLogContext guildLogContext,
        UserLogContext userLogContext,
        Action<BlinkGuild, ulong?> setUlong)
    {
        if (value is null)
        {
            await _unitOfWork.BlinkGuildRepository.UpdatePropertiesAsync(guild,
                entity => setUlong(entity, null));
            await _unitOfWork.SaveChangesAsync();
            logger.LogInformation("{User} Reset ulong property in {Guild} successfully", userLogContext,
                guildLogContext);
            ;
            await RespondSuccessAsync("Value reset", "The ID has been reset to default");
            return;
        }

        if (ulong.TryParse(value, out ulong channelId))
        {
            await _unitOfWork.BlinkGuildRepository.UpdatePropertiesAsync(guild,
                entity => setUlong(entity, channelId));
            await _unitOfWork.SaveChangesAsync();
            logger.LogInformation("{User} Updated ulong property to {Value} in {Guild}", userLogContext,
                guildLogContext, channelId);
            await RespondSuccessAsync("Value updated", "The ID has been updated successfully");
            return;
        }

        logger.LogWarning("{User} Tried invalid ulong value: {Value} in {Guild}", userLogContext, value,
            guildLogContext);
        await RespondErrorAsync("Invalid value", "The value you provided is not a valid ID.");
    }

    /// <summary>
    ///     Sets the specified property colour for the BlinkGuild object.
    /// </summary>
    /// <param name="guild">The BlinkGuild object to update.</param>
    /// <param name="value">The hex code value of the colour. If null or empty, the colour will be reset.</param>
    /// <param name="guildLogContext">The GuildLogContext representing the guild where the operation is performed.</param>
    /// <param name="userLogContext">The UserLogContext representing the user performing the operation.</param>
    /// <param name="setColor">The action to set the colour property in the BlinkGuild object.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SetPropertyColour(BlinkGuild guild,
        string? value,
        GuildLogContext guildLogContext,
        UserLogContext userLogContext,
        Action<BlinkGuild, Color?> setColor)
    {
        if (string.IsNullOrEmpty(value))
        {
            await _unitOfWork.BlinkGuildRepository.UpdatePropertiesAsync(guild,
                entity => setColor(entity, null));
            await _unitOfWork.SaveChangesAsync();
            logger.LogInformation("{User} Reset colour property successfully in {Guild}", userLogContext,
                guildLogContext);
            ;
            await RespondSuccessAsync("Colour reset", "Colour has been reset to default");
            return;
        }

        if (!Color.TryParseHex(value, out Color color))
        {
            logger.LogWarning("{User} Attempted invalid colour value: {Value} in {Guild}", userLogContext, value,
                guildLogContext);
            await RespondErrorAsync("Invalid colour", "The colour you provided is not a valid hex code.");
            return;
        }

        await _unitOfWork.BlinkGuildRepository.UpdatePropertiesAsync(guild, entity =>
            setColor(entity, color));
        await _unitOfWork.SaveChangesAsync();
        logger.LogInformation("{User} Updated colour property to {Value} in {Guild}", userLogContext, value.ToUpper(),
            guildLogContext);
        await RespondSuccessAsync("Colour updated", $"Colour has been changed to {value.ToUpper()}");
    }
}