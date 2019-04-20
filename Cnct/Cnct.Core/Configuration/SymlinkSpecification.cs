using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public class SymlinkSpecification
    {
        [JsonConverter(typeof(LinkCollectionConverter))]
        public string[] Windows { get; set; }

        [JsonConverter(typeof(LinkCollectionConverter))]
        public string[] Osx { get; set; }

        [JsonConverter(typeof(LinkCollectionConverter))]
        public string[] Linux { get; set; }
    }
}
