using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks
{
    public partial class CopyTask
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

        public static partial CopyTask FromTaskSpecification(
            CopyTaskSpecification spec,
            ILogger logger,
            string configDirectoryRoot)
        {
            var fileManagement = new FileManagement();

            return new CopyTask(
                logger,
                new FileSystem(),
                fileManagement.GetFileConfigurations(configDirectoryRoot, spec.Files));
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
            if (this.fileSystem.File.Exists(destination))
            {
                this.Logger.LogWarning($"Destination file '{destination}' already exists. " +
                    $"Delete the destination file first if it should be overwritten");

                return;
            }

            this.fileSystem.File.Copy(sourceFile, destination);
        }
    }
}
