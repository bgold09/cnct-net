using Cnct.Core.Configuration;

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

        public static partial ShellTask FromTaskSpecification(
            ShellTaskSpecification spec,
            ILogger logger,
            string configDirectoryRoot)
        {
            var factory = new ShellInvokerFactory();

            return new ShellTask(
                logger,
                factory.Create(spec.Shell, logger),
                new ShellExecutionOptions(spec.Command, spec.Silent));
        }

        public override Task ExecuteAsync()
        {
            return this.shellInvoker.ExecuteAsync(this.options);
        }
    }
}
