using System.Threading.Tasks;

namespace Cnct.Core.Configuration
{
    public class CnctConfig
    {
        public ICnctActionSpec[] Actions { get; set; }

        public void Validate()
        {
            foreach (var action in this.Actions)
            {
                action.Validate();
            }
        }

        public async Task ExecuteAsync(ILogger logger)
        {
            foreach (var action in this.Actions)
            {
                await action.ExecuteAsync(logger);
            }
        }
    }
}
