using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks.Shell
{
    public class PowerShellInvoker : IShellInvoker
    {
        private static readonly InitialSessionState Iss = InitialSessionState.CreateDefault();

        public async Task ExecuteAsync(ShellTaskSpecification specification)
        {
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            // need this to be configurable
            Iss.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;

            using var powershell = PowerShell.Create(Iss);
            powershell.AddScript(specification.Command);

            powershell.Streams.Error.DataAdded += ToStandardError<ErrorRecord>;
            powershell.Streams.Warning.DataAdded += ToStandardOutput<WarningRecord>;
            if (!specification.Silent)
            {
                powershell.Streams.Information.DataAdded += ToStandardOutput<InformationRecord>;
            }

            await powershell.InvokeAsync();
            if (powershell.HadErrors)
            {
                throw new InvalidOperationException("command failed");
            }
        }

        private static void ToStandardError<T>(object sender, DataAddedEventArgs args)
        {
            ToStream<T>(sender, args, Console.Error);
        }

        private static void ToStandardOutput<T>(object sender, DataAddedEventArgs args)
        {
            ToStream<T>(sender, args, Console.Out);
        }

        private static void ToStream<T>(object sender, DataAddedEventArgs args, TextWriter writer)
        {
            if (!(sender is PSDataCollection<T> collection))
            {
                throw new InvalidOperationException();
            }

            writer.WriteLine(collection[args.Index]);
        }
    }
}
