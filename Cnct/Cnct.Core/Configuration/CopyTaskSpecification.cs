using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Cnct.Core.Validation;

namespace Cnct.Core.Configuration
{
    [CnctActionType("copy")]
    public partial class CopyTaskSpecification : CnctActionSpecBase
    {
        private readonly IFileManagement fileManagement;
        private readonly IFileSystem fileSystem;

        [JsonConverter(typeof(FileSpecificationCollectionConverter))]
        public IReadOnlyDictionary<string, object> Files { get; set; }

        public CopyTaskSpecification(IFileManagement fileManagement, IFileSystem fileSystem)
        {
            this.fileManagement = fileManagement;
            this.fileSystem = fileSystem;
        }

        public CopyTaskSpecification()
            : this(new FileManagement(), new FileSystem())
        {
        }

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();
            if (this.Files == null || this.Files.Count == 0)
            {
                issues.Add(this.CreateValidationError("The collection of files cannot be null or empty."));
                return issues;
            }

            foreach (string sourcePath in this.fileManagement.GetFileConfigurations(configDirectoryRoot, this.Files).Keys)
            {
                if (!this.fileSystem.File.Exists(sourcePath) && !this.fileSystem.Directory.Exists(sourcePath))
                {
                    issues.Add(this.CreateValidationError($"Source path does not exist: {sourcePath}"));
                }
            }

            return issues;
        }

        public override async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var copyTask = new CopyTask(
                logger,
                this.fileSystem,
                this.fileManagement.GetFileConfigurations(configDirectoryRoot, this.Files));

            await copyTask.ExecuteAsync();
        }
    }
}
