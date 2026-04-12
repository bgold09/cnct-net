using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks
{
    internal partial class LinkExpandTask
    {
        private readonly IFileSystem fileSystem;
        private readonly string source;
        private readonly string target;

        public LinkExpandTask(ILogger logger, string source, string target)
            : this(logger, source, target, new FileSystem())
        {
        }

        public LinkExpandTask(ILogger logger, string source, string target, IFileSystem fileSystem)
            : base(logger)
        {
            this.source = source;
            this.target = target;
            this.fileSystem = fileSystem;
        }

        public static LinkExpandTask FromTaskSpecification(
            LinkExpandTaskSpecification spec,
            ILogger logger,
            string configDirectoryRoot)
        {
            var fileSystem = new FileSystem();
            var pathResolver = new PathResolver(fileSystem);
            string source = pathResolver.Resolve(spec.Source, configDirectoryRoot);
            string target = spec.Target.NormalizePath();

            return new LinkExpandTask(logger, source, target, fileSystem);
        }

        public override Task ExecuteAsync()
        {
            if (!this.fileSystem.Directory.Exists(this.source))
            {
                this.Logger.LogWarning($"Source directory '{this.source}' does not exist.");
                return Task.FromResult(0);
            }

            var links = new Dictionary<string, IEnumerable<string>>();
            foreach (string subdirectory in this.fileSystem.Directory.EnumerateDirectories(this.source))
            {
                string name = this.fileSystem.Path.GetFileName(subdirectory);
                string linkPath = this.fileSystem.Path.Combine(this.target, name);
                links[subdirectory] = new[] { linkPath };
            }

            var linkTask = new LinkTask(this.Logger, links, this.fileSystem);
            return linkTask.ExecuteAsync();
        }
    }
}
