using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public class MachineSettings
    {
        public static readonly MachineSettings Empty = new MachineSettings();

        [JsonProperty("tags")]
        [JsonConverter(typeof(StringCollectionConverter))]
        public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
    }
}
