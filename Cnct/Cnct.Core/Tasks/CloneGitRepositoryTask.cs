using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal class CloneGitRepositoryTask : CnctTaskBase
    {
        private readonly IReadOnlyDictionary<string, string> repos;
        private readonly IGitRunner gitRunner;

        public CloneGitRepositoryTask(ILogger logger, IReadOnlyDictionary<string, string> repos)
            : this(logger, repos, new ProcessGitRunner(logger))
        {
        }

        public CloneGitRepositoryTask(ILogger logger, IReadOnlyDictionary<string, string> repos, IGitRunner gitRunner)
            : base(logger)
        {
            this.repos = repos;
            this.gitRunner = gitRunner;
        }

        public override async Task ExecuteAsync()
        {
            foreach (var kvp in this.repos)
            {
                string url = kvp.Key;
                string dest = kvp.Value;
                string gitPath = Path.Combine(dest, ".git");

                if (File.Exists(gitPath) || Directory.Exists(gitPath))
                {
                    this.Logger.LogInformation($"  [GIT] Pulling latest changes in '{dest}'");
                    await this.gitRunner.PullAsync(dest);
                }
                else if (Directory.Exists(dest))
                {
                    this.Logger.LogWarning($"Directory '{dest}' exists but is not a git repository. Skipping.");
                }
                else
                {
                    this.Logger.LogInformation($"  [GIT] Cloning '{url}' to '{dest}'");
                    string parent = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                    {
                        Directory.CreateDirectory(parent);
                    }

                    await this.gitRunner.CloneAsync(url, dest);
                }
            }
        }
    }
}
