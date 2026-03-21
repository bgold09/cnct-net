using System;
using System.IO;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("linkExpand")]
    public sealed partial class LinkExpandTaskSpecification : ICnctActionSpec
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Source))
            {
                throw new InvalidOperationException("A source directory must be specified.");
            }

            if (string.IsNullOrWhiteSpace(this.Target))
            {
                throw new InvalidOperationException("A target directory must be specified.");
            }
        }

        public Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            string source = this.Source.NormalizePath();
            if (!Path.IsPathRooted(source))
            {
                source = Path.Combine(configDirectoryRoot, source);
            }

            string target = this.Target.NormalizePath();
            var linkExpandTask = new LinkExpandTask(logger, source, target);
            return linkExpandTask.ExecuteAsync();
        }
    }
}
