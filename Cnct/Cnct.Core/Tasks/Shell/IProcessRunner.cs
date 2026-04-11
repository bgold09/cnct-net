using System.Diagnostics;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks.Shell
{
    public interface IProcessRunner
    {
        Task ExecuteAsync(
            ProcessStartInfo startInfo,
            ShellExecutionOptions options,
            ILogger logger);
    }
}
