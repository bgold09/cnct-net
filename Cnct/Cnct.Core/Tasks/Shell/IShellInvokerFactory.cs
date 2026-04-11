using static Cnct.Core.Configuration.ShellTaskSpecification;

namespace Cnct.Core.Tasks.Shell
{
    public interface IShellInvokerFactory
    {
        IShellInvoker Create(ShellType shellType, ILogger logger);
    }
}
