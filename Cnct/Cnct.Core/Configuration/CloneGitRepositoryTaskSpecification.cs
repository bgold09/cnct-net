using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("cloneGitRepository")]
    public sealed partial class CloneGitRepositoryTaskSpecification : CnctActionSpecBase
    {
        private readonly IFileSystem fileSystem;
        private readonly IGitRunner gitRunner;

        public CloneGitRepositoryTaskSpecification()
        {
            this.fileSystem = new FileSystem();
        }

        public CloneGitRepositoryTaskSpecification(IGitRunner gitRunner)
            : this(gitRunner, new FileSystem())
        {
        }

        public CloneGitRepositoryTaskSpecification(IGitRunner gitRunner, IFileSystem fileSystem)
        {
            this.gitRunner = gitRunner;
            this.fileSystem = fileSystem;
        }

        [JsonProperty("repos")]
        public IReadOnlyDictionary<string, string> Repos { get; set; }

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();

            if (this.Repos == null || this.Repos.Count == 0)
            {
                issues.Add(this.CreateValidationIssue(
                    ValidationSeverity.Error,
                    "The collection of repositories cannot be null or empty."));
                return issues;
            }

            foreach (var kvp in this.Repos)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    issues.Add(this.CreateValidationIssue(
                        ValidationSeverity.Error,
                        "Each repository entry must have a non-empty URL."));
                }
                else if (!Uri.TryCreate(kvp.Key, UriKind.Absolute, out _))
                {
                    issues.Add(this.CreateValidationIssue(
                        ValidationSeverity.Error,
                        $"Repository URL is not a valid absolute URI: {kvp.Key}"));
                }

                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    issues.Add(this.CreateValidationIssue(
                        ValidationSeverity.Error,
                        $"The destination path for repository '{kvp.Key}' cannot be null or empty."));
                }
            }

            return issues;
        }

        public override Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var normalizedRepos = new Dictionary<string, string>();
            foreach (var kvp in this.Repos)
            {
                string dest = kvp.Value.NormalizePath();
                if (!this.fileSystem.Path.IsPathRooted(dest))
                {
                    dest = this.fileSystem.Path.Combine(configDirectoryRoot, dest);
                }

                normalizedRepos[kvp.Key] = dest;
            }

            var cloneTask = new CloneGitRepositoryTask(
                logger,
                normalizedRepos,
                this.gitRunner ?? new ProcessGitRunner(logger),
                this.fileSystem);

            return cloneTask.ExecuteAsync();
        }

        protected override string GetAdditionalDisplayText() =>
            this.Repos != null && this.Repos.Count > 0
                ? string.Join(", ", this.Repos.Keys)
                : null;
    }
}
