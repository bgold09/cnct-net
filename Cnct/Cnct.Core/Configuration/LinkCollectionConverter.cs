using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cnct.Core.Configuration
{
    public class LinkCollectionConverter : JsonConverter<string[]>
    {
        public override string[] ReadJson(JsonReader reader, Type objectType, string[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            string[] links;
            switch (token.Type)
            {
                case JTokenType.Array:
                    var array = (JArray)token;
                    links = array.Select(e => e.Value<string>()).ToArray();
                    break;
                case JTokenType.String:
                    links = new[] { token.Value<string>() };
                    break;
                case JTokenType.Null:
                    links = Array.Empty<string>();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return links;
        }

        public override void WriteJson(JsonWriter writer, string[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
