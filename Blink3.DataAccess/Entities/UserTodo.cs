using System.ComponentModel.DataAnnotations;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Blink3.DataAccess.Entities;

/// <summary>
///     Represents a UserTodo entity.
/// </summary>
public class UserTodo
{
    /// <summary>
    ///     Gets or sets the Id of the UserTodo.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    ///     Represents the ID of a user.
    /// </summary>
    [Required]
    public required ulong UserId { get; set; }

    /// <summary>
    ///     Represents a label associated with a user's "to do".
    /// </summary>
    /// <remarks>
    ///     The maximum length of the label is 25 characters
    /// </remarks>
    [Required]
    [MaxLength(25)]
    public required string Label { get; set; }

    /// <summary>
    ///     Gets or sets the description of the user "to do".
    /// </summary>
    /// <remarks>
    ///     The maximum length of the description is 50 characters.
    /// </remarks>
    [MaxLength(50)]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user "to do" is complete.
    /// </summary>
    public required bool Complete { get; set; } = false;
}