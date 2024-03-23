using System.Diagnostics.CodeAnalysis;
using Discord;

namespace Blink3.Bot.Extensions;

/// <summary>
///     Contains extension methods for the <see cref="IGuildUser" /> interface.
/// </summary>
public static class GuildUserExtensions
{
    /// <summary>
    ///     Returns the friendly name of a guild user.
    /// </summary>
    /// <param name="user">The guild user.</param>
    /// <returns>The friendly name of the guild user.</returns>
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public static string GetFriendlyName(this IGuildUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.Nickname)) return user.Nickname;
        if (!string.IsNullOrWhiteSpace(user.GlobalName)) return user.GlobalName;
        return user.Username;
    }
}