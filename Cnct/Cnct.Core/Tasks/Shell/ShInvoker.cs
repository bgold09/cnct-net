using System;
using System.Diagnostics;
using System.Text;
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

            var stderr = new StringBuilder();

            if (!specification.Silent)
            {
                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        this.logger.LogInformation(e.Data);
                    }
                };
            }

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stderr.AppendLine(e.Data);
                    if (!specification.Silent)
                    {
                        this.logger.LogWarning(e.Data);
                    }
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string errorOutput = stderr.Length > 0
                    ? stderr.ToString().TrimEnd()
                    : "(no stderr output)";
                throw new InvalidOperationException(
                    $"Command failed (exit code "
                    + $"{process.ExitCode}): {errorOutput}");
            }
        }
    }
}
