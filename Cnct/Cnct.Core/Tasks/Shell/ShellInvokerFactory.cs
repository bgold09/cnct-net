using static Cnct.Core.Configuration.ShellTaskSpecification;

namespace Cnct.Core.Tasks.Shell
{
    public class ShellInvokerFactory : IShellInvokerFactory
    {
        public IShellInvoker Create(ShellType shellType, ILogger logger)
        {
            return shellType switch
            {
                ShellType.PowerShell => new PowerShellInvoker(logger),
                ShellType.Sh => new ShInvoker(logger),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(shellType),
                    shellType,
                    $"Shell type {shellType} is not supported."),
            };
        }
    }
}
