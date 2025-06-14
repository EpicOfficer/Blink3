using Newtonsoft.Json;

namespace Blink3.Core.Helpers;

public class ULongToStringConverter : JsonConverter<ulong>
{
    public override ulong ReadJson(JsonReader reader, Type objectType, ulong existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return ulong.Parse((string)reader.Value!);
    }

    public override void WriteJson(JsonWriter writer, ulong value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}