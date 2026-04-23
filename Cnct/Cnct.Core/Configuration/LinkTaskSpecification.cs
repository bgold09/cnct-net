using Cnct.Core.Validation;

namespace Cnct.Core.Configuration
{
    [CnctActionType("link")]
    public sealed partial class LinkTaskSpecification : CnctActionSpecBase
    {
        private readonly IFileManagement fileManagement;
        private readonly IFileSystem fileSystem;

        public LinkTaskSpecification(IFileManagement fileManagement, IFileSystem fileSystem)
        {
            this.fileManagement = fileManagement;
            this.fileSystem = fileSystem;
        }

        public LinkTaskSpecification()
            : this(new FileManagement(), new FileSystem())
        {
        }

        [JsonConverter(typeof(FileSpecificationCollectionConverter))]
        public IReadOnlyDictionary<string, object> Links { get; set; }

        public override IReadOnlyList<ValidationIssue> Validate(CnctContext context)
        {
            var issues = new List<ValidationIssue>();
            if (this.Links == null || this.Links.Count == 0)
            {
                issues.Add(this.CreateValidationError("The collection of links cannot be null or empty."));
                return issues;
            }

            if (!this.WouldRunOnCurrentMachine(context))
            {
                return issues;
            }

            foreach (string sourcePath in
                this.fileManagement.GetFileConfigurations(context.ConfigDirectoryRoot, this.Links).Keys)
            {
                if (!this.fileSystem.File.Exists(sourcePath) && !this.fileSystem.Directory.Exists(sourcePath))
                {
                    issues.Add(this.CreateValidationError($"Source path does not exist: {sourcePath}"));
                }
            }

            return issues;
        }
    }
}
