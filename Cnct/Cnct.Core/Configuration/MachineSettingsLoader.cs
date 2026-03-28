using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public class MachineSettingsLoader
    {
        private readonly IFileSystem fileSystem;

        public MachineSettingsLoader()
            : this(new FileSystem())
        {
        }

        public MachineSettingsLoader(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public static string GetSettingsFilePath()
        {
            return Path.Combine(GetSettingsDirectory(), "cnct", "settings.json");
        }

        public async Task<MachineSettings> LoadAsync()
        {
            string path = GetSettingsFilePath();
            if (!this.fileSystem.File.Exists(path))
            {
                return MachineSettings.Empty;
            }

            string json = await this.fileSystem.File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<MachineSettings>(json) ?? MachineSettings.Empty;
        }

        private static string GetSettingsDirectory()
        {
            switch (Platform.CurrentPlatform)
            {
                case PlatformType.Windows:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case PlatformType.Linux:
                case PlatformType.OSX:
                    return Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Path.Combine(Platform.Home, ".config");
                default:
                    throw new NotImplementedException($"Platform '{Platform.CurrentPlatform}' is not supported.");
            }
        }
    }
}
