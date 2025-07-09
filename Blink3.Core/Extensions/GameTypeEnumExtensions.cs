using Blink3.Core.Enums;

namespace Blink3.Core.Extensions;

public static class GameTypeEnumExtensions
{
    public static string GetFriendlyName(this GameType? gameType)
    {
        return gameType?.ToString() ?? "Global";
    }
    
    public static string GetFriendlyName(this GameType gameType)
    {
        return gameType.ToString();
    }
}