using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cnct.Core.Configuration
{
    public class EnumCollectionConverter<T> : JsonConverter<IReadOnlyCollection<T>>
        where T : struct
    {
        public override IReadOnlyCollection<T> ReadJson(JsonReader reader, Type objectType, IReadOnlyCollection<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return token.Type switch
            {
                JTokenType.Array => Convert(((JArray)token).Select(e => e.Value<string>())),
                JTokenType.String => Convert(new[] { token.Value<string>() }),
                JTokenType.Null => new HashSet<T>(),
                _ => throw new JsonSerializationException(),
            };
        }

        public override void WriteJson(JsonWriter writer, IReadOnlyCollection<T> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static HashSet<T> Convert(IEnumerable<string> values)
        {
            var result = new HashSet<T>();
            foreach (var item in values)
            {
                if (Enum.TryParse(item, true, out T parsed))
                {
                    result.Add(parsed);
                }
            }

            return result;
        }
    }
}
