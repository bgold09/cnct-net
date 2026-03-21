using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("link")]
    public sealed partial class LinkTaskSpecification : ICnctActionSpec
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

        public void Validate()
        {
            if (this.Links == null || this.Links.Count == 0)
            {
                throw new InvalidOperationException("The collection of links cannot be null or empty.");
            }
        }

        public async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var linkTask = new LinkTask(
                logger,
                this.fileManagement.GetFileConfigurations(configDirectoryRoot, this.Links),
                this.fileSystem);

            await linkTask.ExecuteAsync();
        }
    }
}
