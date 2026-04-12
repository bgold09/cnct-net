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

        private static string GetSettingsDirectory() => Platform.CurrentPlatform switch
        {
            PlatformType.Windows => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            PlatformType.Linux or PlatformType.OSX =>
                Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
                ?? Path.Combine(Platform.Home, ".config"),
            _ => throw new NotImplementedException($"Platform '{Platform.CurrentPlatform}' is not supported."),
        };
    }
}
