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
        private readonly IPathResolver pathResolver;

        public CloneGitRepositoryTaskSpecification()
        {
            this.fileSystem = new FileSystem();
            this.pathResolver = new PathResolver(this.fileSystem);
        }

        public CloneGitRepositoryTaskSpecification(IGitRunner gitRunner)
            : this(gitRunner, new FileSystem())
        {
        }

        public CloneGitRepositoryTaskSpecification(
            IGitRunner gitRunner,
            IFileSystem fileSystem)
            : this(gitRunner, fileSystem, new PathResolver(fileSystem))
        {
        }

        public CloneGitRepositoryTaskSpecification(
            IGitRunner gitRunner,
            IFileSystem fileSystem,
            IPathResolver pathResolver)
        {
            this.gitRunner = gitRunner;
            this.fileSystem = fileSystem;
            this.pathResolver = pathResolver;
        }

        [JsonProperty("repos")]
        public IReadOnlyDictionary<string, string> Repos { get; set; }

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();
            if (this.Repos == null || this.Repos.Count == 0)
            {
                issues.Add(this.CreateValidationError("The collection of repositories cannot be null or empty."));
                return issues;
            }

            foreach (var kvp in this.Repos)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    issues.Add(this.CreateValidationError("Each repository entry must have a non-empty URL."));
                }
                else if (!Uri.TryCreate(kvp.Key, UriKind.Absolute, out _))
                {
                    issues.Add(this.CreateValidationError($"Repository URL is not a valid absolute URI: {kvp.Key}"));
                }

                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    issues.Add(this.CreateValidationError(
                        $"The destination path for repository '{kvp.Key}' cannot be null or empty."));
                }
            }

            return issues;
        }

        public override Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var normalizedRepos = this.ResolveRepoPaths(configDirectoryRoot);

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

        private Dictionary<string, string> ResolveRepoPaths(
            string configDirectoryRoot)
        {
            var resolved = new Dictionary<string, string>();
            foreach (var kvp in this.Repos)
            {
                resolved[kvp.Key] = this.pathResolver.Resolve(
                    kvp.Value,
                    configDirectoryRoot);
            }

            return resolved;
        }
    }
}
