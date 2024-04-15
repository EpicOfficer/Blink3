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

    /// <summary>
    ///     Represents the number of points for a BlinkUser.
    /// </summary>
    public int Points { get; set; }
}