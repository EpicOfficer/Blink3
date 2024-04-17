using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blink3.Core.Helpers;

public class ULongToStringConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? numberAsString = reader.GetString();
        return numberAsString is not null ? ulong.Parse(numberAsString) : 0;
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}