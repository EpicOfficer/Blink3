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

    private Color? _backgroundColour;
    public Color BackgroundColour
    {
        get => _backgroundColour ?? WordleImageConstants.BackgroundColour;
        set => _backgroundColour = value;
    }
    
    private Color? _textColour;
    public Color TextColour
    {
        get => _textColour ?? WordleImageConstants.TextColour;
        set => _textColour = value;
    }
    
    private Color? _correctTileColour;
    public Color CorrectTileColour
    {
        get => _correctTileColour ?? WordleImageConstants.CorrectTileColour;
        set => _correctTileColour = value;
    }
    
    private Color? _misplacedTileColour;
    public Color MisplacedTileColour
    {
        get => _misplacedTileColour ?? WordleImageConstants.MisplacedTileColour;
        set => _misplacedTileColour = value;
    }
    
    private Color? _incorrectTileColour;
    public Color IncorrectTileColour
    {
        get => _incorrectTileColour ?? WordleImageConstants.IncorrectTileColour;
        set => _incorrectTileColour = value;
    }

    public string GetCacheKey()
    {
        return Id.ToString();
    }
}