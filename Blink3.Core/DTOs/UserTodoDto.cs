using System.ComponentModel.DataAnnotations;
using Blink3.DataAccess.Entities;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Blink3.DataAccess.Models;

public class UserTodoDto
{
    /// <summary>
    ///     Represents a label associated with a user's "to do".
    /// </summary>
    [Required(ErrorMessage = "Label is required")]
    [MaxLength(25, ErrorMessage = "Label must not exceed 25 characters")]
    public required string Label { get; set; }

    /// <summary>
    ///     Represents the description of the user "to do".
    /// </summary>
    [MaxLength(50, ErrorMessage = "Description must not exceed 50 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Represents whether user "to do" is complete or not.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Complete { get; set; } = false;

    /// <summary>
    ///     Converts a UserTodoDto object to a UserTodo entity.
    /// </summary>
    /// <param name="userId">The logged in users' ID</param>
    /// <param name="id">The ID of the UserTodo entity. If specified, it will be assigned to the created entity.</param>
    /// <returns>The UserTodo entity created from the UserTodoDto object.</returns>
    public UserTodo ToEntity(ulong userId, int? id = null)
    {
        UserTodo userTodo = new()
        {
            UserId = userId,
            Label = Label,
            Description = Description,
            Complete = Complete
        };

        if (id.HasValue) userTodo.Id = id.Value;

        return userTodo;
    }
}