using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public sealed class LinkAction : ICnctAction
    {
        public string ActionType => "link";

        public bool? Force { get; set; }

        [JsonConverter(typeof(LinkSpecificationCollectionConverter))]
        public IDictionary<string, object> Links { get; set; }
    }
}
