using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cnct.Core.Configuration
{
    public class LinkSpecificationCollectionConverter : JsonConverter<IDictionary<string, object>>
    {
        public override IDictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            var d = new Dictionary<string, object>();
            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                JsonElement token = property.Value;
                object value = token.ValueKind switch
                {
                    JsonValueKind.Object => JsonSerializer.Deserialize<SymlinkSpecification>(ref reader, options),
                    JsonValueKind.String => token.GetString(),
                    JsonValueKind.Null => null,
                    _ => throw new NotImplementedException(),
                };

                d.Add(property.Name, value);
            }

            return d;
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, object> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
