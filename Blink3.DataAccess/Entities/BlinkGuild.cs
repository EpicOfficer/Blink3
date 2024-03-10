using System.ComponentModel.DataAnnotations;
using Blink3.Common.Caching.Interfaces;

namespace Blink3.DataAccess.Entities;

public class BlinkGuild : ICacheKeyIdentifiable
{
    [Key]
    public ulong Id { get; set; }

    public string GetCacheKey()
    {
        return $"{Id.ToString()}";
    }
}