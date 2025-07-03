using Discord;

namespace Blink3.Core.LogContexts;

// ReSharper disable MemberCanBePrivate.Global
public class UserLogContext(IUser user)
{
    public readonly ulong Id = user.Id;
    public readonly string Name = user.GlobalName ?? user.Username;
    
    public override string ToString() => $"{Name} ({Id})";
}
// ReSharper restore MemberCanBePrivate.Global