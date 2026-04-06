using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks.Shell
{
    public class ShInvoker : IShellInvoker
    {
        private readonly ILogger logger;

        public ShInvoker(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task ExecuteAsync(
            ShellTaskSpecification specification)
        {
            if (specification == null)
            {
                throw new ArgumentNullException(
                    nameof(specification));
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add(specification.Command);

            await ProcessRunner.ExecuteAsync(
                startInfo, specification, this.logger);
        }
    }
}
