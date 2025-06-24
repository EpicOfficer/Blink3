using Discord;

namespace Blink3.Core.LogContexts;

// ReSharper disable MemberCanBePrivate.Global
public class GuildLogContext(IGuild guild)
{
    public ulong GuildId => guild.Id;
    public string GuildName => guild.Name;
    
    public override string ToString() => $"Guild: {GuildName} ({GuildId})";
}
// ReSharper restore MemberCanBePrivate.Global