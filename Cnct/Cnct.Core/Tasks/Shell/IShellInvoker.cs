using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks.Shell
{
    public interface IShellInvoker
    {
        Task ExecuteAsync(ShellTaskSpecification specification);
    }
}
