using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [JsonConverter(typeof(CnctActionConverter))]
    public interface ICnctActionSpec
    {
        string ActionType { get; }

        void Validate();

        Task ExecuteAsync(ILogger logger, string configDirectoryRoot);
    }
}
