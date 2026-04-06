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

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException(
                    "Failed to start /bin/sh.");

            Task<string> outputTask =
                process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask =
                process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            if (!specification.Silent
                && !string.IsNullOrWhiteSpace(outputTask.Result))
            {
                this.logger.LogInformation(
                    outputTask.Result.TrimEnd());
            }

            if (process.ExitCode != 0)
            {
                string errorOutput =
                    string.IsNullOrWhiteSpace(errorTask.Result)
                        ? "(no stderr output)"
                        : errorTask.Result.TrimEnd();
                throw new InvalidOperationException(
                    $"Command failed (exit code "
                    + $"{process.ExitCode}): {errorOutput}");
            }

            if (!specification.Silent
                && !string.IsNullOrWhiteSpace(errorTask.Result))
            {
                this.logger.LogWarning(
                    errorTask.Result.TrimEnd());
            }
        }
    }
}
