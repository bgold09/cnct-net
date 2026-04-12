namespace Cnct.Core.Tasks
{
    public class ProcessGitRunner : IGitRunner
    {
        private readonly ILogger logger;

        public ProcessGitRunner(ILogger logger)
        {
            this.logger = logger;
        }

        public Task CloneAsync(string url, string destination)
            => this.RunGitProcessAsync(["clone", url, destination]);

        public Task PullAsync(string repositoryPath)
            => this.RunGitProcessAsync(["-C", repositoryPath, "pull"]);

        private async Task RunGitProcessAsync(string[] arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            foreach (string arg in arguments)
            {
                startInfo.ArgumentList.Add(arg);
            }

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException(
                    "Failed to start git. Ensure git is installed and available on PATH.");

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();
            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(outputTask.Result))
            {
                this.logger.LogInformation(outputTask.Result.TrimEnd());
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"git {string.Join(" ", arguments)} failed (exit code {process.ExitCode}): {errorTask.Result}");
            }

            if (!string.IsNullOrWhiteSpace(errorTask.Result))
            {
                // git writes progress info (e.g. clone progress) to stderr even on success
                this.logger.LogInformation(errorTask.Result.TrimEnd());
            }
        }
    }
}
