using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("linkExpand")]
    public sealed partial class LinkExpandTaskSpecification : CnctActionSpecBase
    {
        private readonly IFileSystem fileSystem;

        public LinkExpandTaskSpecification()
        {
            this.fileSystem = new FileSystem();
        }

        public LinkExpandTaskSpecification(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();
            if (string.IsNullOrWhiteSpace(this.Source))
            {
                issues.Add(this.CreateValidationIssue(
                    ValidationSeverity.Error,
                    "A source directory must be specified."));
            }

            if (string.IsNullOrWhiteSpace(this.Target))
            {
                issues.Add(this.CreateValidationIssue(
                    ValidationSeverity.Error,
                    "A target directory must be specified."));
            }

            if (issues.Count == 0)
            {
                string source = this.Source.NormalizePath();
                if (!this.fileSystem.Path.IsPathRooted(source))
                {
                    source = this.fileSystem.Path.Combine(configDirectoryRoot, source);
                }

                if (!this.fileSystem.Directory.Exists(source))
                {
                    issues.Add(this.CreateValidationIssue(
                        ValidationSeverity.Error,
                        $"Source directory does not exist: {source}"));
                }
            }

            return issues;
        }

        public override Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            string source = this.Source.NormalizePath();
            if (!this.fileSystem.Path.IsPathRooted(source))
            {
                source = this.fileSystem.Path.Combine(configDirectoryRoot, source);
            }

            string target = this.Target.NormalizePath();
            var linkExpandTask = new LinkExpandTask(logger, source, target, this.fileSystem);
            return linkExpandTask.ExecuteAsync();
        }
    }
}
