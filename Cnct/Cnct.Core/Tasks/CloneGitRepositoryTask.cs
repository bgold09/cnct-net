using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal class CloneGitRepositoryTask : CnctTaskBase
    {
        private readonly IReadOnlyDictionary<string, string> repos;

        public CloneGitRepositoryTask(ILogger logger, IReadOnlyDictionary<string, string> repos)
            : base(logger)
        {
            this.repos = repos;
        }

        public override async Task ExecuteAsync()
        {
            foreach (var kvp in this.repos)
            {
                string url = kvp.Key;
                string dest = kvp.Value;

                if (Directory.Exists(Path.Combine(dest, ".git")))
                {
                    this.Logger.LogInformation($"  [GIT] Pulling latest changes in '{dest}'");
                    await this.RunGitAsync(new[] { "-C", dest, "pull" });
                }
                else
                {
                    this.Logger.LogInformation($"  [GIT] Cloning '{url}' to '{dest}'");
                    string parent = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                    {
                        Directory.CreateDirectory(parent);
                    }

                    await this.RunGitAsync(new[] { "clone", url, dest });
                }
            }
        }

        private async Task RunGitAsync(string[] arguments, string workingDirectory = null)
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

            if (workingDirectory != null)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }

            using var process = Process.Start(startInfo);
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
            {
                this.Logger.LogInformation(output.TrimEnd());
            }

            if (process.ExitCode != 0)
            {
                this.Logger.LogError($"git {string.Join(" ", arguments)} failed (exit code {process.ExitCode}): {error}");
            }
            else if (!string.IsNullOrWhiteSpace(error))
            {
                // git writes progress info (e.g. clone progress) to stderr even on success
                this.Logger.LogInformation(error.TrimEnd());
            }
        }
    }
}
