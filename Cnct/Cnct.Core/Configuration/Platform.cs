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

        private static PlatformType GetCurrentPlatformType()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return PlatformType.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return PlatformType.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return PlatformType.OSX;
            }

            // todo
            return PlatformType.Linux;
        }
    }
}
