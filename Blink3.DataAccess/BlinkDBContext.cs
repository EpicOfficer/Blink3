using System.Diagnostics.CodeAnalysis;
using Blink3.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
    ///     Represents the collection of Blink users in the Blink3 application.
    /// </summary>
    public DbSet<BlinkUser> BlinkUsers => Set<BlinkUser>();
    
    /// <summary>
    ///     Represents a user's "to do" items.
    /// </summary>
    public DbSet<UserTodo> UserTodos => Set<UserTodo>();

    /// <summary>
    ///     Represents a Wordle game entity.
    /// </summary>
    public DbSet<Wordle> Wordles => Set<Wordle>();

    /// <summary>
    ///     Represents a word guess entered by a player in the Wordle game.
    /// </summary>
    public DbSet<WordleGuess> WordleGuesses => Set<WordleGuess>();

    /// <summary>
    ///     List of words used in the wordle game.
    /// </summary>
    public DbSet<Word> Words => Set<Word>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wordle>()
            .HasMany(w => w.Guesses)
            .WithOne(g => g.Wordle)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WordleGuess>()
            .OwnsMany(guess => guess.Letters);

        // Composite index on Language, IsSolution and Length
        modelBuilder.Entity<Word>()
            .HasIndex(w => new { w.Language, w.IsSolution, w.Length });

        // Composite index on Text and Language 
        modelBuilder.Entity<Word>()
            .HasIndex(w => new { w.Text, w.Language });

        // Define the value converter
        ValueConverter<Color?, string> colorConverter = new(
            v => v.HasValue ? v.Value.ToPixel<Rgba32>().ToHex() : "",
            v => v != "00000000" && !v.IsNullOrEmpty() ?
                Color.FromPixel(Rgba32.ParseHex(v)) : null);
        
        modelBuilder.Entity<BlinkGuild>(entity =>
        {
            entity.Property(b => b.BackgroundColour).HasConversion(colorConverter);
            entity.Property(b => b.TextColour).HasConversion(colorConverter);
            entity.Property(b => b.CorrectTileColour).HasConversion(colorConverter);
            entity.Property(b => b.MisplacedTileColour).HasConversion(colorConverter);
            entity.Property(b => b.IncorrectTileColour).HasConversion(colorConverter);
        });
    }
}