using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("cloneGitRepository")]
    public sealed partial class CloneGitRepositoryTaskSpecification : ICnctActionSpec
    {
        [JsonProperty("repos")]
        public IReadOnlyDictionary<string, string> Repos { get; set; }

        public void Validate()
        {
            if (this.Repos == null || this.Repos.Count == 0)
            {
                throw new InvalidOperationException("The collection of repositories cannot be null or empty.");
            }
        }

        public Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var normalizedRepos = new Dictionary<string, string>();
            foreach (var kvp in this.Repos)
            {
                normalizedRepos[kvp.Key] = kvp.Value.NormalizePath();
            }

            var cloneTask = new CloneGitRepositoryTask(logger, normalizedRepos);
            return cloneTask.ExecuteAsync();
        }
    }
}
