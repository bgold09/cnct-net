using System;

namespace Cnct.Core
{
    public class ConsoleLogger : ILogger
    {
        private readonly LoggerOptions loggerOptions;

        public ConsoleLogger(LoggerOptions options)
        {
            this.loggerOptions = options;
        }

        public void LogError(string message, Exception exception = null)
        {
            if (exception != null)
            {
                message = $"{message}{Environment.NewLine}Ex: {exception}";
            }

            LogWithColor(message, ConsoleColor.Red);
        }

        public void LogInformation(string message)
        {
            if (!this.loggerOptions.Quiet)
            {
                Console.WriteLine(message);
            }
        }

        public void LogVerbose(string message)
        {
            if (!this.loggerOptions.Quiet && this.loggerOptions.Debug)
            {
                Console.WriteLine(message);
            }
        }

        public void LogWarning(string message)
        {
            LogWithColor(message, ConsoleColor.Yellow);
        }

        private static void LogWithColor(string message, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
