using System.ComponentModel.DataAnnotations;

namespace Blink3.DataAccess.Entities;

public class BlinkGuild
{
    [Key]
    public ulong Id { get; set; }
}