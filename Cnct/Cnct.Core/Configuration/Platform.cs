using System;
using System.Runtime.InteropServices;

namespace Cnct.Core.Configuration
{
    public static class Platform
    {
        private static readonly Lazy<PlatformType> CurrentPlatformLazy = new Lazy<PlatformType>(GetCurrentPlatformType);

        public static string Home
        {
            get
            {
                switch (CurrentPlatform)
                {
                    case PlatformType.Windows:
                        return Environment.GetEnvironmentVariable("USERPROFILE");
                    case PlatformType.Linux:
                    case PlatformType.OSX:
                        return Environment.GetEnvironmentVariable("HOME");
                    default:
                        return null;
                }
            }
        }

        public static PlatformType CurrentPlatform => CurrentPlatformLazy.Value;

        public static bool CurrentPlatformIsUnix { get; } = IsUnix();

        private static bool IsUnix()
        {
            switch (CurrentPlatform)
            {
                case PlatformType.Linux:
                case PlatformType.OSX:
                    return true;
                default:
                    return false;
            }
        }

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
