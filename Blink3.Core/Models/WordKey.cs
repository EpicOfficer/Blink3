namespace Blink3.Core.Models;

public struct WordKey(string language, string text)
{
    public string Language { get; } = language;
    public string Text { get; } = text;

    public override bool Equals(object? obj)
    {
        if (obj is WordKey key) return string.Equals(Language, key.Language) && string.Equals(Text, key.Text);
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Language.GetHashCode();
            hash = hash * 23 + Text.GetHashCode();
            return hash;
        }
    }
}