using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cnct.Core.Configuration
{
    public class CnctActionConverter : JsonConverter<ICnctActionSpec>
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override ICnctActionSpec ReadJson(
            JsonReader reader,
            Type objectType,
            ICnctActionSpec existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            switch (jsonObject["actionType"].Value<string>())
            {
                case "link":
                    return jsonObject.ToObject<LinkTaskSpecification>();

                case "shell":
                    return null;

                default:
                    throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, ICnctActionSpec value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
