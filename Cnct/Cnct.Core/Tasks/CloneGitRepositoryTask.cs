using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks
{
    internal partial class CloneGitRepositoryTask
    {
        private readonly IFileSystem fileSystem;
        private readonly IReadOnlyDictionary<string, string> repos;
        private readonly IGitRunner gitRunner;

        public CloneGitRepositoryTask(ILogger logger, IReadOnlyDictionary<string, string> repos, IGitRunner gitRunner)
            : this(logger, repos, gitRunner, new FileSystem())
        {
        }

        public CloneGitRepositoryTask(ILogger logger, IReadOnlyDictionary<string, string> repos, IGitRunner gitRunner, IFileSystem fileSystem)
            : base(logger)
        {
            this.repos = repos;
            this.gitRunner = gitRunner;
            this.fileSystem = fileSystem;
        }

        public static CloneGitRepositoryTask FromTaskSpecification(
            CloneGitRepositoryTaskSpecification spec,
            ILogger logger,
            string configDirectoryRoot)
        {
            var fileSystem = new FileSystem();
            var pathResolver = new PathResolver(fileSystem);
            var normalizedRepos = new Dictionary<string, string>();
            foreach (var kvp in spec.Repos)
            {
                normalizedRepos[kvp.Key] = pathResolver.Resolve(kvp.Value, configDirectoryRoot);
            }

            return new CloneGitRepositoryTask(
                logger,
                normalizedRepos,
                new ProcessGitRunner(logger),
                fileSystem);
        }

        public override async Task ExecuteAsync()
        {
            foreach (var kvp in this.repos)
            {
                string url = kvp.Key;
                string dest = kvp.Value;
                string gitPath = this.fileSystem.Path.Combine(dest, ".git");

                if (this.fileSystem.File.Exists(gitPath) || this.fileSystem.Directory.Exists(gitPath))
                {
                    this.Logger.LogInformation($"  [GIT] Pulling latest changes in '{dest}'");
                    await this.gitRunner.PullAsync(dest);
                }
                else if (this.fileSystem.Directory.Exists(dest))
                {
                    this.Logger.LogWarning($"Directory '{dest}' exists but is not a git repository. Skipping.");
                }
                else
                {
                    this.Logger.LogInformation($"  [GIT] Cloning '{url}' to '{dest}'");
                    string parent = this.fileSystem.Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(parent) && !this.fileSystem.Directory.Exists(parent))
                    {
                        this.fileSystem.Directory.CreateDirectory(parent);
                    }

                    await this.gitRunner.CloneAsync(url, dest);
                }
            }
        }
    }
}
