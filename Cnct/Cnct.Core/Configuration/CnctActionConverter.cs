using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cnct.Core.Configuration
{
    public class CnctActionConverter : JsonConverter<ICnctActionSpec>
    {
        public override ICnctActionSpec Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument jsonObject = JsonDocument.ParseValue(ref reader);
            string s = jsonObject.RootElement.GetProperty("actionType").GetString();
            Type t = s switch
            {
                "link" => typeof(LinkTaskSpecification),
                //"shell" => null,
                _ => throw new NotImplementedException(),
            };

            return (ICnctActionSpec)JsonSerializer.Deserialize(ref reader, t, options);

            //JsonSerializer.Deserialize<LinkTaskSpecification>(ref reader, options)
        }

        public override void Write(Utf8JsonWriter writer, ICnctActionSpec value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        //public override void WriteJson(JsonWriter writer, ICnctActionSpec value, JsonSerializer serializer)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
