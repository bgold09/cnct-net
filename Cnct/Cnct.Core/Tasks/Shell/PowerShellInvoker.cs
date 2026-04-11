using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

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

            if (OperatingSystem.IsWindows())
            {
                await ExecuteInProcessAsync(options);
            }
            else
            {
                await this.ExecuteAsProcessAsync(options);
            }
        }

        private static async Task ExecuteInProcessAsync(
            ShellExecutionOptions options)
        {
            var sessionState =
                InitialSessionState.CreateDefault2();
            sessionState.ExecutionPolicy =
                Microsoft.PowerShell.ExecutionPolicy.Unrestricted;

            using var powershell =
                PowerShell.Create(sessionState);
            powershell.AddScript(options.Command);

            powershell.Streams.Error.DataAdded +=
                ToStandardError<ErrorRecord>;
            powershell.Streams.Warning.DataAdded +=
                ToStandardOutput<WarningRecord>;
            if (!options.Silent)
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
            ShellExecutionOptions options)
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
            startInfo.ArgumentList.Add(options.Command);

            await this.processRunner.ExecuteAsync(
                startInfo, options, this.logger);
        }
    }
}
