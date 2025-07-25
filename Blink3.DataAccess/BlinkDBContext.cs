using System.Diagnostics.CodeAnalysis;
using Blink3.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess;

/// <summary>
///     Represents the database context for Blink3 application.
/// </summary>
[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
public class BlinkDbContext(DbContextOptions<BlinkDbContext> options) : DbContext(options)
{
    /// <summary>
    ///     Represents a collection of BlinkGuild entities in the BlinkDbContext.
    /// </summary>
    public DbSet<BlinkGuild> BlinkGuilds => Set<BlinkGuild>();

    /// <summary>
    ///     Represents the collection of Blink users in the Blink3 application.
    /// </summary>
    public DbSet<BlinkUser> BlinkUsers => Set<BlinkUser>();

    /// <summary>
    ///     Represents a collection of game statistics entities in the BlinkDbContext.
    /// </summary>
    public DbSet<GameStatistics> GameStatistics => Set<GameStatistics>();

    /// <summary>
    ///     Represents a user's "to do" items.
    /// </summary>
    public DbSet<UserTodo> UserTodos => Set<UserTodo>();

    /// <summary>
    ///     Represents a Wordle game entity.
    /// </summary>
    public DbSet<Wordle> Wordles => Set<Wordle>();
    public DbSet<BlinkMix> BlinkMixes => Set<BlinkMix>();

    /// <summary>
    ///     Represents a word guess entered by a player in the Wordle game.
    /// </summary>
    public DbSet<WordleGuess> WordleGuesses => Set<WordleGuess>();

    /// <summary>
    ///     List of words used in the wordle game.
    /// </summary>
    public DbSet<Word> Words => Set<Word>();

    /// <summary>
    ///     Represents a temporary voice channel entity.
    /// </summary>
    public DbSet<TempVc> TempVcs => Set<TempVc>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wordle>()
            .HasMany(w => w.Guesses)
            .WithOne(g => g.Wordle)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WordleGuess>()
            .OwnsMany(guess => guess.Letters);

        modelBuilder.Entity<Word>(entity =>
        {
            entity.HasIndex(w => new { w.Language, w.IsSolution, w.Length });
            entity.HasIndex(w => new { w.Text, w.Language });
        });

        modelBuilder.Entity<BlinkGuild>(entity =>
        {
            entity.Property(p => p.BackgroundColour).IsRequired(false);
            entity.Property(p => p.TextColour).IsRequired(false);
            entity.Property(p => p.CorrectTileColour).IsRequired(false);
            entity.Property(p => p.MisplacedTileColour).IsRequired(false);
            entity.Property(p => p.IncorrectTileColour).IsRequired(false);
        });

        modelBuilder.Entity<BlinkUser>(entity =>
        {
            entity.HasMany(b => b.GameStatistics)
                .WithOne(g => g.BlinkUser)
                .HasForeignKey(g => g.BlinkUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TempVc>(p => p.HasKey(t => new { t.GuildId, t.ChannelId }));
    }
}