using Discord;

namespace Blink3.Core.LogContexts;

// ReSharper disable MemberCanBePrivate.Global
public class ChannelLogContext(IChannel channel)
{
    public readonly ulong Id = channel.Id;
    public readonly string Name = channel.Name;
    
    public override string ToString() => $"{Name} ({Id})";
}
// ReSharper restore MemberCanBePrivate.Global