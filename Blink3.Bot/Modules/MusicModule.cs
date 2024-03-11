using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;

namespace Blink3.Bot.Modules;

[RequireContext(ContextType.Guild)]
public class MusicModule(IAudioService audioService) : InteractionModuleBase<SocketInteractionContext>
{
    /// <summary>
    ///     Gets the guild player asynchronously.
    /// </summary>
    /// <param name="connectToVoiceChannel">
    ///     a value indicating whether to connect to a voice channel
    /// </param>
    /// <returns>
    ///     a task that represents the asynchronous operation. The task result is the lavalink player.
    /// </returns>
    private async ValueTask<VoteLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true)
    {
        PlayerRetrieveOptions retrieveOptions = new(
            ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

        PlayerResult<VoteLavalinkPlayer> result = await audioService.Players
            .RetrieveAsync(Context, playerFactory: PlayerFactory.Vote, retrieveOptions)
            .ConfigureAwait(false);

        if (result.IsSuccess) return result.Player;
        
        string errorMessage = result.Status switch
        {
            PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
            PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
            _ => "Unknown error.",
        };

        await FollowupAsync(errorMessage).ConfigureAwait(false);
        return null;
    }
    
    /// <summary>
    ///     Plays music asynchronously.
    /// </summary>
    /// <param name="query">the search query</param>
    /// <returns>a task that represents the asynchronous operation</returns>
    [SlashCommand("play", description: "Plays music")]
    public async Task Play(string query)
    {
        await DeferAsync().ConfigureAwait(false);

        VoteLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        LavalinkTrack? track = await audioService.Tracks
            .LoadTrackAsync(query, TrackSearchMode.YouTube)
            .ConfigureAwait(false);

        if (track is null)
        {
            await FollowupAsync("😖 No results.").ConfigureAwait(false);
            return;
        }

        int position = await player.PlayAsync(track).ConfigureAwait(false);

        if (position is 0)
        {
            await FollowupAsync($"🔈 Playing: {track.Uri}").ConfigureAwait(false);
        }
        else
        {
            await FollowupAsync($"🔈 Added to queue: {track.Uri}").ConfigureAwait(false);
        }
    }
}