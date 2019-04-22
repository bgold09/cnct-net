using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace Cnct
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Option configOption = new Option(
                "--config",
                "An option whose argument is parsed as a FileInfo",
                new Argument<FileInfo>());

            Option quietOption = new Option(
                "--quiet",
                "An option whose argument is parsed as a bool",
                new Argument<bool>());

            var rootCommand = new RootCommand();
            rootCommand.Description = "My sample app";
            rootCommand.AddOption(quietOption);
            rootCommand.AddOption(configOption);

            rootCommand.Handler = CommandHandler.Create<bool, FileInfo>((quiet, configFile) =>
            {
                Console.WriteLine($"The value for --bool-option is: {quiet}");
                Console.WriteLine($"The value for --file-option is: {configFile?.FullName ?? "null"}");
            });

            await rootCommand.InvokeAsync(args);
        }
    }
}
