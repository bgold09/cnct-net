using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cnct.Core.Configuration
{
    public class LinkCollectionConverter : JsonConverter<string[]>
    {
        public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            var token = document.RootElement;
            return token.ValueKind switch
            {
                JsonValueKind.Array => token.EnumerateArray().Select(e => e.GetString()).ToArray(),
                JsonValueKind.String => new[] { token.GetString() },
                JsonValueKind.Null => Array.Empty<string>(),
                _ => throw new InvalidOperationException(),
            };
        }

        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
