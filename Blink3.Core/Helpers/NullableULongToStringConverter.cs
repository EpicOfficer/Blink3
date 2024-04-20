using Newtonsoft.Json;

namespace Blink3.Core.Helpers;

public class NullableULongToStringConverter : JsonConverter<ulong?>
{
    public override ulong? ReadJson(JsonReader reader, Type objectType, ulong? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        object? value = reader.Value;
        return value is null ? null : ulong.Parse((string)value);
    }

    public override void WriteJson(JsonWriter writer, ulong? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString() ?? "");
    }
}