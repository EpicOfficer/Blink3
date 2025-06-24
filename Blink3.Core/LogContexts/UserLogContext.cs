using Discord;

namespace Blink3.Core.LogContexts;

// ReSharper disable MemberCanBePrivate.Global
public class UserLogContext(IUser user)
{
    public ulong UserId => user.Id;
    public string UserName => user.GlobalName ?? user.Username;
    
    public override string ToString() => $"User: {UserName} ({UserId})";
}
// ReSharper restore MemberCanBePrivate.Global