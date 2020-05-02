using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cnct.Core.Configuration
{
    public class ActionCollectionConverter : JsonConverter<ActionCollection>
    {
        public override ActionCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument jsonObject = JsonDocument.ParseValue(ref reader);

            var collection = new List<ICnctActionSpec>();
            foreach (JsonElement item in jsonObject.RootElement.EnumerateArray())
            {
                string s = item.GetProperty("actionType").GetString();
                Type t = s switch
                {
                    "link" => typeof(LinkTaskSpecification),
                    //"shell" => null,
                    _ => throw new NotImplementedException(),
                };

                collection.Add((ICnctActionSpec)JsonSerializer.Deserialize(ref reader, t, options));
            }

            return new ActionCollection(collection);
        }

        public override void Write(Utf8JsonWriter writer, ActionCollection value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
