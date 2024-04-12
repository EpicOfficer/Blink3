using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;

namespace Blink3.Core.Helpers;

public class ImageSharpColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }
        
        return Color.ParseHex(reader.GetString() ?? default(Color).ToHex());
    }

    public override void Write(Utf8JsonWriter writer, Color colorToConvert, JsonSerializerOptions options)
    {
        writer.WriteStringValue(colorToConvert.ToHex());
    }
}