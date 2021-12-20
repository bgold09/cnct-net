using System;
using System.Runtime.InteropServices;
using Cnct.Core.Configuration;
using Xunit;

namespace Cnct.Core.Tests
{
    public class PlatformTests
    {
        [Fact]
        public void IdentifyCurrentPlatform()
        {
            PlatformType expectedPlatformType;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                expectedPlatformType = PlatformType.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                expectedPlatformType = PlatformType.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                expectedPlatformType = PlatformType.OSX;
            }
            else
            {
                throw new InvalidOperationException();
            }

            PlatformType actualPlatformType = Platform.CurrentPlatform;

            Assert.Equal(expectedPlatformType, actualPlatformType);
        }

        [Fact]
        public void IsUnixPlatform()
        {
            bool isUnix = Platform.CurrentPlatformIsUnix;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.True(isUnix);
            }
            else
            {
                Assert.False(isUnix);
            }
        }
    }
}
