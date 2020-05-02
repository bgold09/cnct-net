using System;
using System.IO;
using System.Text.Json;

namespace Cnct.Core.Configuration
{
    public class CnctConfigurationParser
    {
        private readonly ILogger logger;

        public CnctConfigurationParser(ILogger logger)
        {
            this.logger = logger;
        }

        public CnctConfig Parse(string configFile)
        {
            configFile = configFile == null
                ? $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}cnct.json"
                : Path.GetFullPath(configFile);

            if (string.IsNullOrWhiteSpace(configFile))
            {
                throw new ArgumentException(
                    "The path to the config file was null or contains only whitespace.",
                    nameof(configFile));
            }

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException("The config file does not exist.", configFile);
            }

            try
            {
                string json = File.ReadAllText(configFile);
                var options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                };

                options.Converters.Add(new ActionCollectionConverter());


                CnctConfig config = JsonSerializer.Deserialize<CnctConfig>(json);
                config.Logger = this.logger;
                config.ConfigRootDirectory = Path.GetDirectoryName(configFile);

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
