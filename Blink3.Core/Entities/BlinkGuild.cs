using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Blink3.Core.Caching.Interfaces;
using Blink3.Core.Constants;
using SixLabors.ImageSharp;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a guild in the Blink3 application.
/// </summary>
public class BlinkGuild : ICacheKeyIdentifiable
{
    private string? _backgroundColour;
    private string? _correctTileColour;
    private string? _incorrectTileColour;
    private string? _misplacedTileColour;
    private string? _textColour;

    /// <summary>
    ///     Represents the identifier of a BlinkGuild entity.
    /// </summary>
    [Key]
    [Required]
    public ulong Id { get; set; }

    /// <summary>
    ///     Gets or sets the wordle background colour.
    /// </summary>
    /// <remarks>
    ///     The wordle background colour represented as a hexadecimal string. If the background colour is not set, it returns
    ///     the default background colour defined in <see cref="WordleImageConstants" />.
    /// </remarks>
    public string BackgroundColour
    {
        get => string.IsNullOrEmpty(_backgroundColour)
            ? string.Concat("#", WordleImageConstants.BackgroundColour.ToHex().AsSpan(0, 6))
            : '#' + _backgroundColour;
        set => _backgroundColour = string.IsNullOrEmpty(value) ? null :
            value.StartsWith('#') ? value.Substring(1, 6) : value[..6];
    }

    /// <summary>
    ///     Gets or sets the wordle text colour.
    /// </summary>
    /// <remarks>
    ///     The wordle text colour represented as a hexadecimal string. If the text colour is not set, it returns the default
    ///     text colour defined in <see cref="WordleImageConstants" />.
    /// </remarks>
    public string TextColour
    {
        get => string.IsNullOrEmpty(_textColour)
            ? string.Concat("#", WordleImageConstants.TextColour.ToHex().AsSpan(0, 6))
            : '#' + _textColour;
        set => _textColour = string.IsNullOrEmpty(value) ? null :
            value.StartsWith('#') ? value.Substring(1, 6) : value[..6];
    }

    /// <summary>
    ///     Gets or sets the wordle correct tile background colour.
    /// </summary>
    /// <remarks>
    ///     The correct tile background colour represented as a hexadecimal string. If the correct tile background colour is
    ///     not set, it returns the default correct tile background colour defined in <see cref="WordleImageConstants" />.
    /// </remarks>
    public string CorrectTileColour
    {
        get => string.IsNullOrEmpty(_correctTileColour)
            ? string.Concat("#", WordleImageConstants.CorrectTileColour.ToHex().AsSpan(0, 6))
            : '#' + _correctTileColour;
        set => _correctTileColour = string.IsNullOrEmpty(value) ? null :
            value.StartsWith('#') ? value.Substring(1, 6) : value[..6];
    }

    /// <summary>
    ///     Gets or sets the wordle misplaced tile background colour.
    /// </summary>
    /// <remarks>
    ///     The misplaced tile background colour represented as a hexadecimal string. If the misplaced tile background colour
    ///     is not set, it returns the default misplaced tile background colour defined in <see cref="WordleImageConstants" />.
    /// </remarks>
    public string MisplacedTileColour
    {
        get => string.IsNullOrEmpty(_misplacedTileColour)
            ? string.Concat("#", WordleImageConstants.MisplacedTileColour.ToHex().AsSpan(0, 6))
            : '#' + _misplacedTileColour;
        set => _misplacedTileColour = string.IsNullOrEmpty(value) ? null :
            value.StartsWith('#') ? value.Substring(1, 6) : value[..6];
    }

    /// <summary>
    ///     Gets or sets the wordle incorrect tile background colour.
    /// </summary>
    /// <remarks>
    ///     The incorrect tile background colour represented as a hexadecimal string. If the incorrect tile background colour
    ///     is not set, it returns the default incorrect tile background colour defined in <see cref="WordleImageConstants" />.
    /// </remarks>
    public string IncorrectTileColour
    {
        get => string.IsNullOrEmpty(_incorrectTileColour)
            ? string.Concat("#", WordleImageConstants.IncorrectTileColour.ToHex().AsSpan(0, 6))
            : '#' + _incorrectTileColour;
        set => _incorrectTileColour = string.IsNullOrEmpty(value) ? null :
            value.StartsWith('#') ? value.Substring(1, 6) : value[..6];
    }

    /// <summary>
    ///     Gets or sets the ID of the channel to be used for staff logging.
    /// </summary>
    public ulong? LoggingChannelId { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the category where Temporary VCs will be created
    /// </summary>
    public ulong? TemporaryVcCategoryId { get; set; }

    [NotMapped]
    public Color ParsedBackgroundColor => Color.ParseHex(BackgroundColour);
    
    [NotMapped]
    public Color ParsedTextColour => Color.ParseHex(TextColour);
    
    [NotMapped]
    public Color ParsedCorrectTileColour => Color.ParseHex(CorrectTileColour);
    
    [NotMapped]
    public Color ParsedMisplacedTileColour => Color.ParseHex(MisplacedTileColour);
    
    [NotMapped]
    public Color ParsedIncorrectTileColour => Color.ParseHex(IncorrectTileColour);
    
    public string GetCacheKey()
    {
        return Id.ToString();
    }
}