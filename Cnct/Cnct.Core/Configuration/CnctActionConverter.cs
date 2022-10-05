using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cnct.Core.Configuration
{
    public partial class CnctActionConverter : JsonConverter<ICnctActionSpec>
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
            ICnctActionSpec spec = GetActionSpecFromType(jsonObject["actionType"].Value<string>());
            if (spec != null)
            {
                serializer?.Populate(jsonObject.CreateReader(), spec);
            }

            return spec;
        }

        public override void WriteJson(JsonWriter writer, ICnctActionSpec value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
