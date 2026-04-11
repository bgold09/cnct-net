using System;
using System.Collections.Generic;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("cloneGitRepository")]
    public sealed partial class CloneGitRepositoryTaskSpecification : CnctActionSpecBase
    {
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

        protected override string GetAdditionalDisplayText() =>
            this.Repos != null && this.Repos.Count > 0
                ? string.Join(", ", this.Repos.Keys)
                : null;
    }
}
