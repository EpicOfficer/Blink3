namespace Blink3.Core.Models;

public class WordDetails
{
    public string Word { get; set; } = string.Empty;
    public List<WordDefinition> Definitions { get; set; } = [];
}