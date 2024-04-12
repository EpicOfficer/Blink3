using System.ComponentModel.DataAnnotations;
using Blink3.Core.Caching.Interfaces;
using Blink3.Core.Constants;
using Blink3.Core.Interfaces;
using SixLabors.ImageSharp;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a guild in the Blink3 application.
/// </summary>
public class BlinkGuild : ICacheKeyIdentifiable
{
    /// <summary>
    ///     Represents the identifier of a BlinkGuild entity.
    /// </summary>
    [Key]
    [Required]
    public ulong Id { get; set; }

    private string? _backgroundColour = null;
    public string BackgroundColour
    {
        get => string.IsNullOrEmpty(_backgroundColour) ? WordleImageConstants.BackgroundColour.ToHex() : _backgroundColour;
        set => _backgroundColour = string.IsNullOrEmpty(value) ? null : value;
    }
    
    private string? _textColour = null;
    public string TextColour
    {
        get => string.IsNullOrEmpty(_textColour) ? WordleImageConstants.TextColour.ToHex() : _textColour;
        set => _textColour = string.IsNullOrEmpty(value) ? null : value;
    }
    
    private string? _correctTileColour = null;
    public string CorrectTileColour
    {
        get => string.IsNullOrEmpty(_correctTileColour) ? WordleImageConstants.CorrectTileColour.ToHex() : _correctTileColour;
        set => _correctTileColour = string.IsNullOrEmpty(value) ? null : value;
    }
    
    private string? _misplacedTileColour = null;
    public string MisplacedTileColour
    {
        get => string.IsNullOrEmpty(_misplacedTileColour) ? WordleImageConstants.MisplacedTileColour.ToHex() : _misplacedTileColour;
        set => _misplacedTileColour = string.IsNullOrEmpty(value) ? null : value;
    }
    
    private string? _incorrectTileColour = null;
    public string IncorrectTileColour
    {
        get => string.IsNullOrEmpty(_incorrectTileColour) ? WordleImageConstants.IncorrectTileColour.ToHex() : _incorrectTileColour;
        set => _incorrectTileColour = string.IsNullOrEmpty(value) ? null : value;
    }
    
    public ulong? LoggingChannelId { get; set; }

    public string GetCacheKey()
    {
        return Id.ToString();
    }
}