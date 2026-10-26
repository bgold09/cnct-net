namespace Cnct.Core.Tasks.Shell
{
    public class PowerShellInvoker : IShellInvoker
    {
        private readonly ILogger logger;
        private readonly IProcessRunner processRunner;

        public PowerShellInvoker(ILogger logger = null)
            : this(logger, new DefaultProcessRunner())
        {
        }

        public PowerShellInvoker(
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
                FileName = "pwsh",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NoLogo");
            if (OperatingSystem.IsWindows())
            {
                startInfo.ArgumentList.Add("-ExecutionPolicy");
                startInfo.ArgumentList.Add("Unrestricted");
            }

            startInfo.ArgumentList.Add("-Command");
            startInfo.ArgumentList.Add("$ErrorActionPreference = 'Stop'; " + options.Command);

            await this.processRunner.ExecuteAsync(
                startInfo, options, this.logger);
        }
    }
}
