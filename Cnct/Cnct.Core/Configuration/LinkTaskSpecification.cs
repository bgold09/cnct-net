using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("link")]
    public sealed partial class LinkTaskSpecification : CnctActionSpecBase
    {
        private readonly IFileManagement fileManagement;
        private readonly IFileSystem fileSystem;

        public LinkTaskSpecification(IFileManagement fileManagement)
            : this(fileManagement, new FileSystem())
        {
        }

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

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();

            if (this.Links == null || this.Links.Count == 0)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    this.ActionType,
                    this.Label,
                    "The collection of links cannot be null or empty."));
                return issues;
            }

            foreach (string sourcePath in this.fileManagement.GetFileConfigurations(configDirectoryRoot, this.Links).Keys)
            {
                if (!this.fileSystem.File.Exists(sourcePath) && !this.fileSystem.Directory.Exists(sourcePath))
                {
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Error,
                        this.ActionType,
                        this.Label,
                        $"Source path does not exist: {sourcePath}"));
                }
            }

            return issues;
        }

        public override async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var linkTask = new LinkTask(
                logger,
                this.fileManagement.GetFileConfigurations(configDirectoryRoot, this.Links),
                this.fileSystem);

            await linkTask.ExecuteAsync();
        }
    }
}
