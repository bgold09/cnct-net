using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

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

            await this.processRunner.ExecuteAsync(
                startInfo, specification, this.logger);
        }
    }
}
