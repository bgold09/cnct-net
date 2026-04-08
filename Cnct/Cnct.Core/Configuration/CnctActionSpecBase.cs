using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public abstract class CnctActionSpecBase : ICnctActionSpec
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("tags")]
        [JsonConverter(typeof(StringCollectionConverter))]
        public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();

        [JsonProperty("os")]
        [JsonConverter(typeof(EnumCollectionConverter<PlatformType>))]
        public IReadOnlyCollection<PlatformType> PlatformType { get; set; }

        public abstract string ActionType { get; }

        public virtual string GetDisplayText()
        {
            if (!string.IsNullOrEmpty(this.Label))
            {
                return $"{this.ActionType}: {this.Label}";
            }

            string extra = this.GetAdditionalDisplayText();
            return string.IsNullOrEmpty(extra)
                ? this.ActionType
                : $"{this.ActionType}: {extra}";
        }

        public bool ShouldExecuteOnCurrentPlatform()
        {
            return this.PlatformType == null
                || this.PlatformType.Count == 0
                || this.PlatformType.Contains(Platform.CurrentPlatform);
        }

        public abstract void Validate();

        public abstract Task ExecuteAsync(ILogger logger, string configDirectoryRoot);

        protected virtual string GetAdditionalDisplayText() => null;
    }
}
