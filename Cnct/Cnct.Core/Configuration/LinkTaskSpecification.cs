using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("link")]
    public sealed partial class LinkTaskSpecification : ICnctActionSpec
    {
        private readonly IFileManagement fileManagement;

        public LinkTaskSpecification(IFileManagement fileManagement)
        {
            this.fileManagement = fileManagement;
        }

        public LinkTaskSpecification()
        {
            this.fileManagement = new FileManagement();
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
                logger, this.fileManagement.GetFileConfigurations(configDirectoryRoot, this.Links));

            await linkTask.ExecuteAsync();
        }
    }
}
