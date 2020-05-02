using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cnct.Core.Configuration
{
    public class SymlinkCollectionConverter : JsonConverter<SymlinkCollection>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var b = base.CanConvert(typeToConvert);

            return b;
        }

        public override SymlinkCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            var token = document.RootElement;
            var list = token.ValueKind switch
            {
                JsonValueKind.Array => token.EnumerateArray().Select(e => e.GetString()).ToArray(),
                JsonValueKind.String => new[] { token.GetString() },
                JsonValueKind.Null => Array.Empty<string>(),
                _ => throw new JsonException(),
            };

            return new SymlinkCollection(list);
        }

        public override void Write(Utf8JsonWriter writer, SymlinkCollection value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
