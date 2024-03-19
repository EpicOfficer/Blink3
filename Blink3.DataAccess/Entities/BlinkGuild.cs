using System.ComponentModel.DataAnnotations;
using Blink3.Common.Caching.Interfaces;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Blink3.DataAccess.Entities;

/// <summary>
/// Represents a guild in the Blink3 application.
/// </summary>
public class BlinkGuild : ICacheKeyIdentifiable
{
    /// <summary>
    /// Represents the identifier of a BlinkGuild entity.
    /// </summary>
    [Key]
    public required ulong Id { get; set; }

    public string GetCacheKey() => Id.ToString();
}