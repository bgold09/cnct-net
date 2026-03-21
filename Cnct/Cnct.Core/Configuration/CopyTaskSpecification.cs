using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Cnct.Core.Tasks;

namespace Cnct.Core.Configuration
{
    [CnctActionType("copy")]
    public partial class CopyTaskSpecification : ICnctActionSpec
    {
        private readonly IFileManagement fileManagement;
        private readonly IFileSystem fileSystem;

        [JsonConverter(typeof(FileSpecificationCollectionConverter))]
        public IReadOnlyDictionary<string, object> Files { get; set; }

        public CopyTaskSpecification(IFileManagement fileManagement)
            : this(fileManagement, new FileSystem())
        {
        }

        public CopyTaskSpecification(IFileManagement fileManagement, IFileSystem fileSystem)
        {
            this.fileManagement = fileManagement;
            this.fileSystem = fileSystem;
        }

        public CopyTaskSpecification()
            : this(new FileManagement(), new FileSystem())
        {
        }

        public void Validate()
        {
            if (this.Files == null || this.Files.Count == 0)
            {
                throw new InvalidOperationException("The collection of files cannot be null or empty.");
            }
        }

        public async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var copyTask = new CopyTask(
                logger,
                this.fileSystem,
                this.fileManagement.GetFileConfigurations(configDirectoryRoot, this.Files));

            await copyTask.ExecuteAsync();
        }
    }
}
