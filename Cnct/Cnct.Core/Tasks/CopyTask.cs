using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal class CopyTask : CnctTaskBase
    {
        private readonly IDictionary<string, IEnumerable<string>> fileMap;

        public CopyTask(ILogger logger, IDictionary<string, IEnumerable<string>> fileMap)
            : base(logger)
        {
            this.fileMap = fileMap;
        }

        public override Task ExecuteAsync()
        {
            foreach (var kvp in this.fileMap)
            {
                string sourceFile = kvp.Key;
                IEnumerable<string> destinationPaths = kvp.Value;
                foreach (string destination in destinationPaths)
                {
                    this.CopyFile(sourceFile, destination);
                }
            }

            return Task.CompletedTask;
        }

        private void CopyFile(string sourceFile, string destination)
        {
            this.Logger.LogInformation($"  [COPY] {sourceFile} -> {destination}");
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, destination);
            }
            else
            {
                this.Logger.LogWarning($"Source '{sourceFile}' does not exist.");
            }
        }
    }
}
