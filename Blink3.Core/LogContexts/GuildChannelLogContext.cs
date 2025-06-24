using Discord;

namespace Blink3.Core.LogContexts;

// ReSharper disable MemberCanBePrivate.Global
public class GuildChannelLogContext(IGuildChannel channel)
{
    public ulong ChannelId => channel.Id;
    public string ChannelName => channel.Name;
    public ulong GuildId => channel.Guild.Id;
    public string GuildName => channel.Guild.Name;
    
    public override string ToString() => $"Channel: {ChannelName} ({ChannelId}) in Guild: {GuildName} ({GuildId})";
}
// ReSharper restore MemberCanBePrivate.Global