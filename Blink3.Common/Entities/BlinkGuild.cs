using System.ComponentModel.DataAnnotations;

namespace Blink3.Common.Entities;

public class BlinkGuild
{
    [Key]
    public ulong Id { get; set; }
    public DateTime JoinedDate { get; set; }
}