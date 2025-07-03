using Discord;

namespace Blink3.Core.LogContexts;

// ReSharper disable MemberCanBePrivate.Global
public class GuildChannelLogContext(IGuildChannel channel)
{
    public readonly ChannelLogContext Channel = new(channel);
    public readonly GuildLogContext Guild = new(channel.Guild);

    public override string ToString() =>
        $"{Channel.Name} ({Channel.Id}) in {Guild.Name} ({Guild.Id})";
}
// ReSharper restore MemberCanBePrivate.Global