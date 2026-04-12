namespace Cnct.Core.Configuration
{
    public class MachineSettings
    {
        public static readonly MachineSettings Empty = new();

        [JsonProperty("tags")]
        [JsonConverter(typeof(StringCollectionConverter))]
        public IReadOnlyCollection<string> Tags { get; set; } = [];
    }
}
