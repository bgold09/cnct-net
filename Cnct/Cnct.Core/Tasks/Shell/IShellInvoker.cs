using System.Threading.Tasks;

namespace Cnct.Core.Tasks.Shell
{
    public interface IShellInvoker
    {
        Task ExecuteAsync(ShellExecutionOptions options);
    }
}
