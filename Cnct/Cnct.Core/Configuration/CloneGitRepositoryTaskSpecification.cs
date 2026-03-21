using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("cloneGitRepository")]
    public sealed partial class CloneGitRepositoryTaskSpecification : ICnctActionSpec
    {
        private readonly IGitRunner gitRunner;

        public CloneGitRepositoryTaskSpecification()
        {
        }

        public CloneGitRepositoryTaskSpecification(IGitRunner gitRunner)
        {
            this.gitRunner = gitRunner;
        }

        [JsonProperty("repos")]
        public IReadOnlyDictionary<string, string> Repos { get; set; }

        public void Validate()
        {
            if (this.Repos == null || this.Repos.Count == 0)
            {
                throw new InvalidOperationException("The collection of repositories cannot be null or empty.");
            }

            foreach (var kvp in this.Repos)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    throw new InvalidOperationException("Each repository entry must have a non-empty URL.");
                }

                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    throw new InvalidOperationException($"The destination path for repository '{kvp.Key}' cannot be null or empty.");
                }
            }
        }

        public Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var normalizedRepos = new Dictionary<string, string>();
            foreach (var kvp in this.Repos)
            {
                string dest = kvp.Value.NormalizePath();
                if (!Path.IsPathRooted(dest))
                {
                    dest = Path.Combine(configDirectoryRoot, dest);
                }

                normalizedRepos[kvp.Key] = dest;
            }

            var cloneTask = new CloneGitRepositoryTask(logger, normalizedRepos, this.gitRunner ?? new ProcessGitRunner(logger));
            return cloneTask.ExecuteAsync();
        }
    }
}
