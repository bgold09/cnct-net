using System.Threading.Tasks;

namespace Cnct.Core.Configuration
{
    public class CnctConfig
    {
        public ICnctActionSpec[] Actions { get; set; }

        public async Task ExecuteAsync()
        {
            foreach (var action in this.Actions)
            {
                await action.ExecuteAsync();
            }
        }
    }
}
