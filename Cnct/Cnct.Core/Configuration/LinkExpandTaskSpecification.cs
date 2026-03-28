using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
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

        public override void Validate()
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
