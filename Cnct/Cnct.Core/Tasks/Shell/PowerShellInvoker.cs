using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks.Shell
{
    public class PowerShellInvoker : IShellInvoker
    {
        private readonly ILogger logger;

        public PowerShellInvoker(ILogger logger = null)
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

            if (OperatingSystem.IsWindows())
            {
                await ExecuteInProcessAsync(specification);
            }
            else
            {
                await this.ExecuteAsProcessAsync(specification);
            }
        }

        private static async Task ExecuteInProcessAsync(
            ShellTaskSpecification specification)
        {
            var sessionState =
                InitialSessionState.CreateDefault2();
            sessionState.ExecutionPolicy =
                Microsoft.PowerShell.ExecutionPolicy.Unrestricted;

            using var powershell =
                PowerShell.Create(sessionState);
            powershell.AddScript(specification.Command);

            powershell.Streams.Error.DataAdded +=
                ToStandardError<ErrorRecord>;
            powershell.Streams.Warning.DataAdded +=
                ToStandardOutput<WarningRecord>;
            if (!specification.Silent)
            {
                powershell.Streams.Information.DataAdded +=
                    ToStandardOutput<InformationRecord>;
            }

            await powershell.InvokeAsync();
            if (powershell.HadErrors)
            {
                throw new InvalidOperationException(
                    "command failed");
            }
        }

        private static void ToStandardError<T>(
            object sender, DataAddedEventArgs args)
        {
            ToStream<T>(sender, args, Console.Error);
        }

        private static void ToStandardOutput<T>(
            object sender, DataAddedEventArgs args)
        {
            ToStream<T>(sender, args, Console.Out);
        }

        private static void ToStream<T>(
            object sender,
            DataAddedEventArgs args,
            TextWriter writer)
        {
            if (!(sender is PSDataCollection<T> collection))
            {
                throw new InvalidOperationException();
            }

            writer.WriteLine(collection[args.Index]);
        }

        private async Task ExecuteAsProcessAsync(
            ShellTaskSpecification specification)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NoLogo");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(specification.Command);

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException(
                    "Failed to start pwsh.");

            var stderr = new StringBuilder();

            if (!specification.Silent)
            {
                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        this.logger?.LogInformation(e.Data);
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
                        this.logger?.LogWarning(e.Data);
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
