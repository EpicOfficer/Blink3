using System.Diagnostics.CodeAnalysis;
using Blink3.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess;

/// <summary>
///     Represents the database context for Blink3 application.
/// </summary>
[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
public class BlinkDbContext : DbContext
{
    /// <summary>
    ///     Represents the database context for Blink3 application.
    /// </summary>
    public BlinkDbContext(DbContextOptions<BlinkDbContext> options) : base(options)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    ///     Represents a collection of BlinkGuild entities in the BlinkDbContext.
    /// </summary>
    public DbSet<BlinkGuild> BlinkGuilds => Set<BlinkGuild>();

    /// <summary>
    ///     Represents a user's "to do" items.
    /// </summary>
    public DbSet<UserTodo> UserTodos => Set<UserTodo>();

    /// <summary>
    ///     Represents a Wordle game entity.
    /// </summary>
    public DbSet<Wordle> Wordles => Set<Wordle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wordle>()
            .OwnsOne(wordle => wordle.CurrentGuess)
            .OwnsMany(guess => guess.Letters);
    }
}