using System.Text;

namespace Cnct.Core.Tasks.Shell
{
    public class DefaultProcessRunner : IProcessRunner
    {
        public async Task ExecuteAsync(
            ProcessStartInfo startInfo,
            ShellExecutionOptions options,
            ILogger logger)
        {
            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException(
                    $"Failed to start {startInfo.FileName}.");

            var stderr = new StringBuilder();

            if (!options.Silent)
            {
                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        logger.LogInformation(e.Data);
                    }
                };
            }

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stderr.AppendLine(e.Data);
                    if (!options.Silent)
                    {
                        logger.LogWarning(e.Data);
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
                    + $"{process.ExitCode}): "
                    + errorOutput);
            }
        }
    }
}
