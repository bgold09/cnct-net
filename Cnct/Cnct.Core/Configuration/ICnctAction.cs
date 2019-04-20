using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [JsonConverter(typeof(CnctActionConverter))]
    public interface ICnctAction
    {
        string ActionType { get; }
    }
}
