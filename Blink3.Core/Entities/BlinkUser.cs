using System.ComponentModel.DataAnnotations;

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a global application user
/// </summary>
public class BlinkUser
{
    /// <summary>
    ///     Gets or sets the Id of the BlinkUser.
    /// </summary>
    [Key]
    [Required]
    public ulong Id { get; set; }

    public ICollection<GameStatistics> GameStatistics { get; set; } = [];
}