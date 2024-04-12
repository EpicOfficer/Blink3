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
        [ChoiceDisplay("Wordle Text Colour")] WordleTextColour,

        [ChoiceDisplay("Wordle Correct Tile Colour")]
        WordleCorrectTileColour,

        [ChoiceDisplay("Wordle Misplaced Tile Colour")]
        WordleMisplacedTileColour,

        [ChoiceDisplay("Wordle Incorrect Tile Colour")]
        WordleIncorrectTileColour
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
            default:
                await RespondErrorAsync("Unrecognised setting", "The setting you provided is not recognised.");
                break;
        }
    }

    private async Task SetPropertyColour(BlinkGuild guild, string? value, Action<Color> setColor)
    {
        if (string.IsNullOrEmpty(value))
        {
            setColor(default);
            await _blinkGuildRepository.UpdateAsync(guild);
            await RespondSuccessAsync("Colour reset", "Colour has been reset");
            return;
        }

        if (!Color.TryParseHex(value, out Color color))
        {
            await RespondErrorAsync("Invalid colour", "The colour you provided is not a valid hex code.");
            return;
        }

        setColor(color);
        await _blinkGuildRepository.UpdateAsync(guild);
        await RespondSuccessAsync("Colour changed", $"Colour has been changed to {value.ToUpper()}");
    }
}