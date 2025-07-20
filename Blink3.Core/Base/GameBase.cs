using System.ComponentModel.DataAnnotations;
using Blink3.Core.Enums;
using Blink3.Core.Interfaces;

namespace Blink3.Core.Base;

public abstract class GameBase
{
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    ///     Game type to identify the specific game implementation (e.g., Wordle, BlinkMix).
    /// </summary>
    public abstract GameType Type { get; }

    public ulong ChannelId { get; set; }
    
    /// <summary>
    ///     List of player IDs participating in the game.
    /// </summary>
    public ICollection<ulong> Players { get; set; } = new List<ulong>();

    /// <summary>
    ///     Adds a player to the game if not already present.
    /// </summary>
    public void AddPlayer(ulong playerId)
    {
        if (!Players.Contains(playerId))
        {
            Players.Add(playerId);
        }
    }
}