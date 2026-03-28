using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public abstract class CnctActionSpecBase : ICnctActionSpec
    {
        [JsonProperty("tags")]
        [JsonConverter(typeof(StringCollectionConverter))]
        public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();

        public abstract string ActionType { get; }

        public abstract void Validate();

        public abstract Task ExecuteAsync(ILogger logger, string configDirectoryRoot);
    }
}
