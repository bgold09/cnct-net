namespace Cnct.Core.Configuration
{
    public class CnctConfig
    {
        [JsonProperty(ItemConverterType = typeof(CnctActionConverter))]
        public ICnctActionSpec[] Actions { get; set; }
    }
}
