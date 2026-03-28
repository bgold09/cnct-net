using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cnct.Core.Configuration
{
    public class StringCollectionConverter : JsonConverter<IReadOnlyCollection<string>>
    {
        public override IReadOnlyCollection<string> ReadJson(
            JsonReader reader,
            Type objectType,
            IReadOnlyCollection<string> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return token.Type switch
            {
                JTokenType.Array => new HashSet<string>(((JArray)token).Select(e => e.Value<string>())),
                JTokenType.String => new HashSet<string> { token.Value<string>() },
                JTokenType.Null => new HashSet<string>(),
                _ => throw new JsonSerializationException(),
            };
        }

        public override void WriteJson(JsonWriter writer, IReadOnlyCollection<string> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
