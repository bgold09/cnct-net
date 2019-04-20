using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cnct.Core
{
    public class CnctConfig
    {
        [JsonProperty("actions")]
        public ICnctAction[] Actions { get; set; }
    }

    [JsonConverter(typeof(CnctActionConverter))]
    public interface ICnctAction
    {
        string ActionType { get; }
    }

    public sealed class LinkAction : ICnctAction
    {
        public string ActionType => "link";

        public bool? Force { get; set; }

        [JsonConverter(typeof(LinkSpecificationCollectionConverter))]
        public IDictionary<string, object> Links { get; set; }
    }

    public class SymlinkSpecification
    {
        public string Windows { get; set; }
        public string Osx { get; set; }
        public string Linux { get; set; }
    }

    public class LinkSpecificationCollectionConverter : JsonConverter<IDictionary<string, object>>
    {
        public override IDictionary<string, object> ReadJson(JsonReader reader, Type objectType, IDictionary<string, object> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var d = new Dictionary<string, object>();

            foreach (var kvp in jsonObject)
            {
                string target = kvp.Key;
                JToken token = kvp.Value;

                object value;
                switch (token.Type)
                {
                    case JTokenType.Null:
                        value = null;
                        break;

                    case JTokenType.String:
                        value = token.Value<string>();
                        break;

                    case JTokenType.Object:
                        value = new SymlinkSpecification();
                        var v = (JObject)token;

                        serializer.Populate(v.CreateReader(), value);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                d.Add(target, value);
            }

            return d;
        }

        public override void WriteJson(JsonWriter writer, IDictionary<string, object> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class CnctActionConverter : JsonConverter<ICnctAction>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override ICnctAction ReadJson(JsonReader reader, Type objectType, ICnctAction existingValue, bool hasExistingValue, JsonSerializer serializer)
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
