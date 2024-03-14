using Blink3.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess;

/// <summary>
/// Represents the database context for Blink3 application.
/// </summary>
public class BlinkDbContext(DbContextOptions<BlinkDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Represents a collection of BlinkGuild entities in the BlinkDbContext.
    /// </summary>
    public DbSet<BlinkGuild> BlinkGuilds { get; set; }

    /// <summary>
    /// Represents a user's "to do" items.
    /// </summary>
    public DbSet<UserTodo> UserTodos { get; set; }
}