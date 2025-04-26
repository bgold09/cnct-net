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

        [JsonConverter(typeof(FileSpecificationCollectionConverter))]
        public IReadOnlyDictionary<string, object> Files { get; set; }

        public CopyTaskSpecification(IFileManagement fileManagement)
        {
            this.fileManagement = fileManagement;
        }

        public CopyTaskSpecification()
        {
            this.fileManagement = new FileManagement();
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
                new FileSystem(),
                this.fileManagement.GetFileConfigurations(configDirectoryRoot, this.Files));

            await copyTask.ExecuteAsync();
        }
    }
}
