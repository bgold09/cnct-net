using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks
{
    public interface IActionRunner
    {
        Task ExecuteAsync(ICnctActionSpec spec, ILogger logger, string configDirectoryRoot);
    }
}
