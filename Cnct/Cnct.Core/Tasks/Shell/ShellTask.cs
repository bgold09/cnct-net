using System.Threading.Tasks;
using Cnct.Core.Configuration;
using static Cnct.Core.Configuration.ShellTaskSpecification;

namespace Cnct.Core.Tasks.Shell
{
    internal partial class ShellTask
    {
        private readonly IShellInvoker shellInvoker;
        private readonly ShellExecutionOptions options;

        public ShellTask(
            ILogger logger,
            IShellInvoker shellInvoker,
            ShellExecutionOptions options)
            : base(logger)
        {
            this.shellInvoker = shellInvoker;
            this.options = options;
        }

        public static ShellTask FromTaskSpecification(
            ShellTaskSpecification spec,
            ILogger logger,
            string configDirectoryRoot)
        {
            var factory = new ShellInvokerFactory();
            IShellInvoker invoker = factory.Create(
                spec.Shell, logger);
            var options = new ShellExecutionOptions(
                spec.Command, spec.Silent);
            return new ShellTask(logger, invoker, options);
        }

        public override Task ExecuteAsync()
        {
            return this.shellInvoker.ExecuteAsync(this.options);
        }
    }
}
