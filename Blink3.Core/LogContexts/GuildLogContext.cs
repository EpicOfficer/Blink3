using Discord;

namespace Blink3.Core.LogContexts;

// ReSharper disable MemberCanBePrivate.Global
public class GuildLogContext(IGuild guild)
{
    public readonly ulong Id = guild.Id;
    public readonly string Name = guild.Name;
    
    public override string ToString() => $"{Name} ({Id})";
}
// ReSharper restore MemberCanBePrivate.Global