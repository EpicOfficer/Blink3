using System.ComponentModel.DataAnnotations;
using Blink3.Core.Base;
using Blink3.Core.Enums;

namespace Blink3.Core.Entities;

public class BlinkMix : GameBase
{
    public override GameType Type => GameType.BlinkMix;

    [Required]
    [MaxLength(10)]
    public string Solution { get; set; } = string.Empty;
    
}