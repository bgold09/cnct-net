namespace Cnct.Core.Tasks.Shell
{
    public class ShInvoker : IShellInvoker
    {
        private readonly ILogger logger;
        private readonly IProcessRunner processRunner;

        public ShInvoker(ILogger logger)
            : this(logger, new DefaultProcessRunner())
        {
        }

        public ShInvoker(
            ILogger logger, IProcessRunner processRunner)
        {
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public async Task ExecuteAsync(
            ShellExecutionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options));
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add(options.Command);

            await this.processRunner.ExecuteAsync(
                startInfo, options, this.logger);
        }
    }
}
