using System.ComponentModel.DataAnnotations;
using Blink3.Core.Enums;

namespace Blink3.Core.Entities;

public class GameStatistics
{
    [Required]
    [Key]
    public int Id { get; set; }
    
    public ulong BlinkUserId { get; set; } // FK to BlinkUser
    public BlinkUser BlinkUser { get; set; } = null!;
    
    public GameType Type { get; set; }
    
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int CurrentStreak { get; set; }
    public int MaxStreak { get; set; }
    public DateTime? LastActivity { get; set; }
}