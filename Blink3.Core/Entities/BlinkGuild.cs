using System.ComponentModel.DataAnnotations;
using Blink3.Core.Caching.Interfaces;
using Blink3.Core.Constants;

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
            ? WordleImageConstants.BackgroundColour.ToHex()
            : _backgroundColour;
        set => _backgroundColour = string.IsNullOrEmpty(value) ? null : value;
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
        get => string.IsNullOrEmpty(_textColour) ? WordleImageConstants.TextColour.ToHex() : _textColour;
        set => _textColour = string.IsNullOrEmpty(value) ? null : value;
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
            ? WordleImageConstants.CorrectTileColour.ToHex()
            : _correctTileColour;
        set => _correctTileColour = string.IsNullOrEmpty(value) ? null : value;
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
            ? WordleImageConstants.MisplacedTileColour.ToHex()
            : _misplacedTileColour;
        set => _misplacedTileColour = string.IsNullOrEmpty(value) ? null : value;
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
            ? WordleImageConstants.IncorrectTileColour.ToHex()
            : _incorrectTileColour;
        set => _incorrectTileColour = string.IsNullOrEmpty(value) ? null : value;
    }

    /// <summary>
    ///     Gets or sets the ID of the channel to be used for staff logging.
    /// </summary>
    public ulong? LoggingChannelId { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the category where Temporary VCs will be created
    /// </summary>
    public ulong? TemporaryVcCategoryId { get; set; }
    
    public string GetCacheKey()
    {
        return Id.ToString();
    }
}