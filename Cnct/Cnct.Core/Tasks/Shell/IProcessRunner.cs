using System.Diagnostics;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks.Shell
{
    public interface IProcessRunner
    {
        Task ExecuteAsync(
            ProcessStartInfo startInfo,
            ShellTaskSpecification specification,
            ILogger logger);
    }
}
