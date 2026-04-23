using Cnct.Core.Validation;

namespace Cnct.Core.Configuration
{
    [CnctActionType("linkExpand")]
    public sealed partial class LinkExpandTaskSpecification : CnctActionSpecBase
    {
        private readonly IFileSystem fileSystem;
        private readonly IPathResolver pathResolver;

        public LinkExpandTaskSpecification(
            IFileSystem fileSystem,
            IPathResolver pathResolver)
        {
            this.fileSystem = fileSystem;
            this.pathResolver = pathResolver;
        }

        public LinkExpandTaskSpecification()
            : this(new FileSystem(), new PathResolver(new FileSystem()))
        {
        }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        public override IReadOnlyList<ValidationIssue> Validate(CnctContext context)
        {
            var issues = new List<ValidationIssue>();
            if (string.IsNullOrWhiteSpace(this.Source))
            {
                issues.Add(this.CreateValidationError("A source directory must be specified."));
            }

            if (string.IsNullOrWhiteSpace(this.Target))
            {
                issues.Add(this.CreateValidationError("A target directory must be specified."));
            }

            if (issues.Count == 0 && this.WouldRunOnCurrentMachine(context))
            {
                string source = this.pathResolver.Resolve(this.Source, context.ConfigDirectoryRoot);
                if (!this.fileSystem.Directory.Exists(source))
                {
                    issues.Add(this.CreateValidationError($"Source directory does not exist: {source}"));
                }
            }

            return issues;
        }
    }
}
