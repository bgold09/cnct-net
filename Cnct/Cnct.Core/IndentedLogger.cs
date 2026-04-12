namespace Cnct.Core
{
    internal class IndentedLogger : ILogger
    {
        private const string Indent = "  ";
        private readonly ILogger inner;

        public IndentedLogger(ILogger inner)
        {
            this.inner = inner;
        }

        public void LogInformation(string message) =>
            this.inner.LogInformation($"{Indent}{message}");

        public void LogVerbose(string message) =>
            this.inner.LogVerbose($"{Indent}{message}");

        public void LogWarning(string message) =>
            this.inner.LogWarning(message);

        public void LogError(string message, Exception exception = null) =>
            this.inner.LogError(message, exception);

        public void LogStart(string message) =>
            this.inner.LogStart(message);

        public void LogFinish(string message) =>
            this.inner.LogFinish(message);
    }
}
