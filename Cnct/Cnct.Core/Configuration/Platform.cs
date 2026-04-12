namespace Cnct.Core.Configuration
{
    public static class Platform
    {
        private static readonly Lazy<PlatformType> CurrentPlatformLazy = new(GetCurrentPlatformType);

        public static string Home => CurrentPlatform switch
        {
            PlatformType.Windows => Environment.GetEnvironmentVariable("USERPROFILE"),
            PlatformType.Linux or PlatformType.OSX => Environment.GetEnvironmentVariable("HOME"),
            _ => null,
        };

        public static PlatformType CurrentPlatform => CurrentPlatformLazy.Value;

        public static bool CurrentPlatformIsUnix { get; } = IsUnix();

        private static bool IsUnix() => CurrentPlatform switch
        {
            PlatformType.Linux or PlatformType.OSX => true,
            _ => false,
        };

        private static PlatformType GetCurrentPlatformType()
        {
            if (OperatingSystem.IsWindows())
            {
                return PlatformType.Windows;
            }

            if (OperatingSystem.IsLinux())
            {
                return PlatformType.Linux;
            }

            if (OperatingSystem.IsMacOS())
            {
                return PlatformType.OSX;
            }

            // todo
            return PlatformType.Linux;
        }
    }
}
