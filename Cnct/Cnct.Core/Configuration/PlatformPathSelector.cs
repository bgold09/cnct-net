using System;

namespace Cnct.Core.Configuration
{
    public class PlatformPathSelector : IPlatformPathSelector
    {
        public bool IsCurrentPlatformUnix => Platform.CurrentPlatformIsUnix;

        public string[] GetPlatformPaths(FileSpecification spec)
        {
            return Platform.CurrentPlatform switch
            {
                PlatformType.Windows => spec.Windows,
                PlatformType.Linux => spec.Linux,
                PlatformType.OSX => spec.Osx,
                _ => throw new NotImplementedException(),
            };
        }

        public string[] GetUnixPaths(FileSpecification spec)
        {
            return spec.Unix;
        }
    }
}
