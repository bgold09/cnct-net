namespace Cnct.Core.Configuration
{
    public class LinkCollectionConverter : JsonConverter<string[]>
    {
        public override string[] ReadJson(JsonReader reader, Type objectType, string[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            return token.Type switch
            {
                JTokenType.Array => ((JArray)token).Select(e => e.Value<string>()).ToArray(),
                JTokenType.String => [token.Value<string>()],
                JTokenType.Null => [],
                _ => throw new InvalidOperationException(),
            };
        }

        public override void WriteJson(JsonWriter writer, string[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
