using System;
using System.IO;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public class CnctConfigurationParser
    {
        private readonly ILogger logger;

        public CnctConfigurationParser(ILogger logger)
        {
            this.logger = logger;
        }

        public CnctConfig Parse(FileInfo configFile)
        {
            if (!configFile.Exists)
            {
                throw new FileNotFoundException("The config file does not exist.", configFile.FullName);
            }

            try
            {
                string json = File.ReadAllText(configFile.FullName);
                CnctConfig config = JsonConvert.DeserializeObject<CnctConfig>(json);
                config.Logger = this.logger;

                return config;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to parse '{configFile.FullName}'.", ex);
                throw;
            }
        }
    }
}
