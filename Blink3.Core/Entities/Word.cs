using System.ComponentModel.DataAnnotations;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a word used in the wordle game
/// </summary>
public class Word
{
    private string _text = string.Empty;
    
    /// <summary>
    ///     Gets or sets the Id of the Word.
    /// </summary>
    /// <remarks>
    ///     This property represents the unique identifier of the Word entity.
    /// </remarks>
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    ///     The text for the word
    /// </summary>
    [Required]
    [MaxLength(8)]
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            Length = _text.Length;
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the word is a possible solution in the Wordle game.
    /// </summary>
    /// <remarks>
    ///     The IsSolution property represents whether a word is a possible solution in the Wordle game.
    ///     When this property is set to true, it indicates that the word is a possible solution.
    /// </remarks>
    [Required]
    public bool IsSolution { get; set; }

    /// <summary>
    ///     Represents the language code of the word.  e.g. 'en'
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Language { get; set; } = "en";

    /// <summary>
    ///     Represents the length property of a Word object.
    /// </summary>
    /// <remarks>
    ///     The Length property represents the number of characters in the Text property of a Word object.
    ///     This property is automatically calculated based on the value of the Text property.
    /// </remarks>
    public int Length { get; private set; }
}