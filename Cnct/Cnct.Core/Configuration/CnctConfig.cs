using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public class CnctConfig
    {
        [JsonIgnore]
        public ILogger Logger { get; set; }

        public ICnctActionSpec[] Actions { get; set; }

        public void Validate()
        {
            foreach (var action in this.Actions)
            {
                action.Validate();
            }
        }

        public async Task ExecuteAsync()
        {
            foreach (var action in this.Actions)
            {
                await action.ExecuteAsync(this.Logger);
            }
        }
    }
}
