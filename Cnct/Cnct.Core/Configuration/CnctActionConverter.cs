using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cnct.Core.Configuration
{
    public class CnctActionConverter : JsonConverter<ICnctAction>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override ICnctAction ReadJson(
            JsonReader reader,
            Type objectType,
            ICnctAction existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            ICnctAction action;

            switch (jsonObject["actionType"].Value<string>())
            {
                case "link":
                    action = new LinkAction();
                    break;

                case "shell":
                    return null;

                default:
                    throw new NotImplementedException();
            }

            serializer.Populate(jsonObject.CreateReader(), action);

            return action;
        }

        public override void WriteJson(JsonWriter writer, ICnctAction value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
