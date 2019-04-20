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
            ICnctActionSpec action;

            switch (jsonObject["actionType"].Value<string>())
            {
                case "link":
                    action = new LinkTaskSpecification();
                    break;

                case "shell":
                    return null;

                default:
                    throw new NotImplementedException();
            }

            serializer.Populate(jsonObject.CreateReader(), action);

            return action;
        }

        public override void WriteJson(JsonWriter writer, ICnctActionSpec value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
