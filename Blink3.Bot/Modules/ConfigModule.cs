using Blink3.Core.Constants;
using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;
using Discord;
using Discord.Interactions;
using Color = SixLabors.ImageSharp.Color;

namespace Blink3.Bot.Modules;

[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[CommandContextType(InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.GuildInstall)]
[Group("config", "Configure the bot for this server")]
public class ConfigModule(IBlinkGuildRepository blinkGuildRepository)
    : BlinkModuleBase<IInteractionContext>(blinkGuildRepository)
{
    private readonly IBlinkGuildRepository _blinkGuildRepository = blinkGuildRepository;

    public enum SettingsEnum
    {
        [ChoiceDisplay("Wordle Background Colour")]
        WordleBackgroundColour,
        
        [ChoiceDisplay("Wordle Text Colour")]
        WordleTextColour,

        [ChoiceDisplay("Wordle Correct Tile Colour")]
        WordleCorrectTileColour,

        [ChoiceDisplay("Wordle Misplaced Tile Colour")]
        WordleMisplacedTileColour,

        [ChoiceDisplay("Wordle Incorrect Tile Colour")]
        WordleIncorrectTileColour,
        
        [ChoiceDisplay("Staff Logging Channel")]
        StaffLoggingChannel
    }
    
    [SlashCommand("set", "Change or reset config values")]
    public async Task Set(SettingsEnum setting, string? value = null)
    {
        BlinkGuild guild = await FetchConfig();

        switch (setting)
        {
            case SettingsEnum.WordleBackgroundColour:
                await SetPropertyColour(guild, value,
                    colour => guild.BackgroundColour = colour);
                break;
            case SettingsEnum.WordleTextColour:
                await SetPropertyColour(guild, value,
                    colour => guild.TextColour = colour);
                break;
            case SettingsEnum.WordleCorrectTileColour:
                await SetPropertyColour(guild, value,
                    colour => guild.CorrectTileColour = colour);
                break;
            case SettingsEnum.WordleMisplacedTileColour:
                await SetPropertyColour(guild, value,
                    colour => guild.MisplacedTileColour = colour);
                break;
            case SettingsEnum.WordleIncorrectTileColour:
                await SetPropertyColour(guild, value,
                    colour => guild.IncorrectTileColour = colour);
                break;
            case SettingsEnum.StaffLoggingChannel:
                await SetPropertyChannel(guild, value, channel => guild.LoggingChannelId = channel);
                break;
            default:
                await RespondErrorAsync("Unrecognised setting", "The setting you provided is not recognised.");
                break;
        }
    }

    /// <summary>
    ///     Sets the value of a channel property in the BlinkGuild object.
    /// </summary>
    /// <param name="guild">The BlinkGuild object to modify.</param>
    /// <param name="value">The new value of the channel property. Use null to reset it to default.</param>
    /// <param name="setChannel">The action to set the channel property in the BlinkGuild object.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SetPropertyChannel(BlinkGuild guild, string? value, Action<ulong?> setChannel)
    {
        if (value is null)
        {
            setChannel(null);
            await _blinkGuildRepository.UpdateAsync(guild);
            await RespondSuccessAsync("Value reset", "Channel/Category ID has been reset to default");
            return;
        }
        
        if (ulong.TryParse(value, out ulong channelId))
        {
            setChannel(channelId);
            await _blinkGuildRepository.UpdateAsync(guild);
            await RespondSuccessAsync("Value updated", "Channel/Category ID has been updated");
            return;
        }
        
        await RespondErrorAsync("Invalid channel", "The value you provided is not a valid channel ID.");
    }

    /// <summary>
    ///     Sets the specified property colour for the BlinkGuild object.
    /// </summary>
    /// <param name="guild">The BlinkGuild object.</param>
    /// <param name="value">The hex code value of the colour. If null or empty, the colour will be reset.</param>
    /// <param name="setColor">The action to set the colour property.</param>
    private async Task SetPropertyColour(BlinkGuild guild, string? value, Action<Color> setColor)
    {
        if (string.IsNullOrEmpty(value))
        {
            setColor(default);
            await _blinkGuildRepository.UpdateAsync(guild);
            await RespondSuccessAsync("Colour reset", "Colour has been reset to default");
            return;
        }

        if (!Color.TryParseHex(value, out Color color))
        {
            await RespondErrorAsync("Invalid colour", "The colour you provided is not a valid hex code.");
            return;
        }

        setColor(color);
        await _blinkGuildRepository.UpdateAsync(guild);
        await RespondSuccessAsync("Colour updated", $"Colour has been changed to {value.ToUpper()}");
    }
}