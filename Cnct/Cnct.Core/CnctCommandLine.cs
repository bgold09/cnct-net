using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Cnct.Core.Validation;

namespace Cnct.Core
{
    public static class CnctCommandLine
    {
        public static async Task<int> Invoke(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Description = "A cross-platform bootstrapping tool. Connect your dotfiles / cnct the dots!",
                Handler = CommandHandler.Create((Func<FileInfo, bool, bool, bool, Task<int>>)ExecuteAsync),
            };

            string configOptDescription = "Path to a configuration file. If not supplied, a file called 'cnct.json' "
                + "in the current directory will be used, if it can be found.";
            Option[] options = new[]
            {
                CreateOption<FileInfo>('c', "config", configOptDescription),
                CreateOption<bool>('q', "quiet", "Suppress all output other than errors."),
                CreateOption<bool>('d', "debug", "Output additional debug information."),
                CreateOption<bool>('v', "validate", "Validate the config file and output results as JSON."),
            };

            foreach (var option in options)
            {
                rootCommand.AddOption(option);
            }

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<int> ExecuteAsync(FileInfo config, bool quiet, bool debug, bool validate)
        {
            var logger = new ConsoleLogger(new LoggerOptions(quiet, debug));
            var parser = new CnctConfigurationParser(logger);
            string configFilePath = config?.FullName ?? $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}cnct.json";

            CnctConfig cnctConfig = parser.Parse(configFilePath);
            cnctConfig.MachineTags = (await new MachineSettingsLoader().LoadAsync()).Tags;

            ConfigValidationResult validation = cnctConfig.Validate();
            if (validate)
            {
                Console.WriteLine(validation.ToJson());
                return validation.IsValid ? 0 : 1;
            }

            if (!validation.IsValid)
            {
                foreach (var issue in validation.Issues.Where(i => i.Severity == ValidationSeverity.Error))
                {
                    logger.LogError(issue.Message);
                }

                return 1;
            }

            bool result = await cnctConfig.ExecuteAsync();

            return result ? 0 : 1;
        }

        private static Option CreateOption<T>(char shortName, string longName, string description)
        {
            return new Option(
                new[] { $"-{shortName}", $"--{longName}" },
                description)
            {
                Argument = new Argument<T>(),
            };
        }
    }
}
