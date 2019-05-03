using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core
{
    public static class CnctCommandLine
    {
        public static async Task Invoke(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Description = "A cross-platform bootstrapping tool. Connect your dotfiles / cnct the dots!",
                Handler = CommandHandler.Create<FileInfo, bool, bool>(ExecuteAsync)
            };

            string configOptDescription = "Path to a configuration file. If not supplied, a file called 'cnct.json' "
                + "in the current directory will be used, if it can be found.";
            Option[] options = new[]
            {
                CreateOption<FileInfo>('c', "config", configOptDescription),
                CreateOption<bool>('q', "quiet", "Suppress all output other than errors."),
                CreateOption<bool>('d', "debug", "Output additional debug information."),
            };

            foreach (var option in options)
            {
                rootCommand.AddOption(option);
            }

            await rootCommand.InvokeAsync(args);
        }

        private static async Task<int> ExecuteAsync(FileInfo config, bool quiet, bool debug)
        {
            var logger = new ConsoleLogger(new LoggerOptions(quiet, debug));
            var parser = new CnctConfigurationParser(logger);
            string configFilePath = config?.FullName ?? $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}cnct.json";

            CnctConfig cnctConfig = parser.Parse(configFilePath);
            bool result = await cnctConfig.ExecuteAsync();

            return result ? 0 : 1;
        }

        private static Option CreateOption<T>(char shortName, string longName, string description)
        {
            return new Option(
                new[] { $"-{shortName}", $"--{longName}" },
                description,
                new Argument<T>());
        }
    }
}
