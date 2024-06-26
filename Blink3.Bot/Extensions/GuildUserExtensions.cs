using System.Diagnostics.CodeAnalysis;
using Discord;

namespace Blink3.Bot.Extensions;

/// <summary>
///     Contains extension methods for the <see cref="IGuildUser" /> interface.
/// </summary>
public static class GuildUserExtensions
{
    /// <summary>
    ///     Returns the friendly name of a discord user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The friendly name of the discord user.</returns>
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public static string GetFriendlyName(this IUser user)
    {
        if (user is IGuildUser guildUser &&
            !string.IsNullOrWhiteSpace(guildUser.Nickname))
            return guildUser.Nickname;

        if (!string.IsNullOrWhiteSpace(user.GlobalName)) return user.GlobalName;
        return user.Username;
    }
}