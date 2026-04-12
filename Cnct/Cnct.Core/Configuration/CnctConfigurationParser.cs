namespace Cnct.Core.Configuration
{
    public class CnctConfigurationParser
    {
        private readonly IFileSystem fileSystem;
        private readonly ILogger logger;

        public CnctConfigurationParser(ILogger logger)
            : this(logger, new FileSystem())
        {
        }

        public CnctConfigurationParser(ILogger logger, IFileSystem fileSystem)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
        }

        public CnctConfig Parse(string configFile)
        {
            configFile = configFile == null
                ? $"{this.fileSystem.Directory.GetCurrentDirectory()}{this.fileSystem.Path.DirectorySeparatorChar}cnct.json"
                : this.fileSystem.Path.GetFullPath(configFile);

            if (string.IsNullOrWhiteSpace(configFile))
            {
                throw new ArgumentException(
                    "The path to the config file was null or contains only whitespace.",
                    nameof(configFile));
            }

            if (!this.fileSystem.File.Exists(configFile))
            {
                throw new FileNotFoundException("The config file does not exist.", configFile);
            }

            try
            {
                string json = this.fileSystem.File.ReadAllText(configFile);
                CnctConfig config = JsonConvert.DeserializeObject<CnctConfig>(json);
                config.Logger = this.logger;
                config.ConfigRootDirectory = this.fileSystem.Path.GetDirectoryName(configFile);

                return config;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to parse '{configFile}'.", ex);
                throw;
            }
        }
    }
}
