using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    public class CopyTask : CnctTaskBase
    {
        private readonly IFileSystem fileSystem;
        private readonly IDictionary<string, IEnumerable<string>> fileMap;

        public CopyTask(
            ILogger logger,
            IFileSystem fileSystem,
            IDictionary<string, IEnumerable<string>> fileMap)
            : base(logger)
        {
            this.fileSystem = fileSystem;
            this.fileMap = fileMap;
        }

        public override Task ExecuteAsync()
        {
            foreach (var kvp in this.fileMap)
            {
                string sourceFile = kvp.Key;
                IEnumerable<string> destinationPaths = kvp.Value;
                if (!this.fileSystem.File.Exists(sourceFile))
                {
                    this.Logger.LogWarning($"Source '{sourceFile}' does not exist.");
                }
                else
                {
                    foreach (string destination in destinationPaths)
                    {
                        this.CopyFile(sourceFile, destination);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private void CopyFile(string sourceFile, string destination)
        {
            this.Logger.LogInformation($"  [COPY] {sourceFile} -> {destination}");
            this.fileSystem.File.Copy(sourceFile, destination);
        }
    }
}
