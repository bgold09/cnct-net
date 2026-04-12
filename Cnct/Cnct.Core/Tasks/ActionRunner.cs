using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks
{
    public partial class ActionRunner : IActionRunner
    {
        public Task ExecuteAsync(ICnctActionSpec spec, ILogger logger, string configDirectoryRoot)
            => DispatchAsync(spec, logger, configDirectoryRoot);
    }
}
