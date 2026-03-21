using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal class LinkExpandTask : CnctTaskBase
    {
        private readonly string source;
        private readonly string target;

        public LinkExpandTask(ILogger logger, string source, string target)
            : base(logger)
        {
            this.source = source;
            this.target = target;
        }

        public override Task ExecuteAsync()
        {
            if (!Directory.Exists(this.source))
            {
                this.Logger.LogWarning($"Source directory '{this.source}' does not exist.");
                return Task.FromResult(0);
            }

            var links = new Dictionary<string, IEnumerable<string>>();
            foreach (string subdirectory in Directory.GetDirectories(this.source))
            {
                string name = Path.GetFileName(subdirectory);
                string linkPath = Path.Combine(this.target, name);
                links[subdirectory] = new[] { linkPath };
            }

            var linkTask = new LinkTask(this.Logger, links);
            return linkTask.ExecuteAsync();
        }
    }
}
