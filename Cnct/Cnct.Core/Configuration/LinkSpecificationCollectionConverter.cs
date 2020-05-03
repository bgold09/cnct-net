using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cnct.Core.Configuration
{
    public class LinkSpecificationCollectionConverter : JsonConverter<IDictionary<string, object>>
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override IDictionary<string, object> ReadJson(
            JsonReader reader,
            Type objectType,
            IDictionary<string, object> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var d = new Dictionary<string, object>();

            foreach (var kvp in jsonObject)
            {
                string target = kvp.Key;
                JToken token = kvp.Value;
                object value = token.Type switch
                {
                    JTokenType.Null => null,
                    JTokenType.String => token.Value<string>(),
                    JTokenType.Object => token.ToObject<SymlinkSpecification>(),
                    _ => throw new NotImplementedException(),
                };

                d.Add(target, value);
            }

            return d;
        }

        public override void WriteJson(JsonWriter writer, IDictionary<string, object> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
