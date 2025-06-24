using Discord;

namespace Blink3.Core.LogContexts;

// ReSharper disable MemberCanBePrivate.Global
public class ChannelLogContext(IChannel channel)
{
    public ulong ChannelId => channel.Id;
    public string ChannelName => channel.Name;
    
    public override string ToString() => $"Channel: {ChannelName} ({ChannelId})";
}
// ReSharper restore MemberCanBePrivate.Global