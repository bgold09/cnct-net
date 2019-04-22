using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core
{
    public static class CommandLineOptions
    {
        public static async Task T(string[] args)
        {
            string configOptDescription = "Path to a configuration file. If not supplied, a file called 'cnct.json' "
                + "in the current directory will be used, if it can be found.";

            Option[] options = new[]
            {
                CreateOption<FileInfo>('c', "config", configOptDescription),
                CreateOption<bool>('q', "quiet", "Suppress all output other than errors."),
                CreateOption<bool>('d', "debug", "Output additional debug information."),
            };

            var rootCommand = new RootCommand
            {
                Description = "A cross-platform bootstrapping tool. Connect your dotfiles / cnct the dots!"
            };

            foreach (Option option in options)
            {
                rootCommand.AddOption(option);
            }

            rootCommand.Handler = CommandHandler.Create<FileInfo, bool, bool>(async (configFile, quiet, debug) =>
            {
                var logger = new ConsoleLogger(new LoggerOptions(quiet, debug));
                var parser = new CnctConfigurationParser(logger);
                CnctConfig config = parser.Parse(configFile);

                await config.ExecuteAsync();
            });

            await rootCommand.InvokeAsync(args);
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
