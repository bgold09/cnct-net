using System;
using Cnct.Core.Configuration;
using Xunit;

namespace Cnct.Core.Tests
{
    public class PlatformPathSelectorTests
    {
        [Fact]
        public void GetPlatformPathsReturnsCurrentPlatformPaths()
        {
            var selector = new PlatformPathSelector();
            var spec = new FileSpecification
            {
                Windows = new[] { @"C:\Users\test\file" },
                Linux = new[] { "/home/test/file" },
                Osx = new[] { "/Users/test/file" },
            };

            string[] result = selector.GetPlatformPaths(spec);

            string[] expected = Platform.CurrentPlatform switch
            {
                PlatformType.Windows => spec.Windows,
                PlatformType.Linux => spec.Linux,
                PlatformType.OSX => spec.Osx,
                _ => throw new NotImplementedException(),
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPlatformPathsReturnsNullWhenPlatformNotSet()
        {
            var selector = new PlatformPathSelector();
            var spec = new FileSpecification();

            string[] result = selector.GetPlatformPaths(spec);

            Assert.Null(result);
        }

        [Fact]
        public void GetUnixPathsReturnsUnixProperty()
        {
            var selector = new PlatformPathSelector();
            var spec = new FileSpecification
            {
                Unix = new[] { "/home/test/file" },
            };

            string[] result = selector.GetUnixPaths(spec);

            Assert.Equal(spec.Unix, result);
        }

        [Fact]
        public void GetUnixPathsReturnsNullWhenNotSet()
        {
            var selector = new PlatformPathSelector();
            var spec = new FileSpecification();

            string[] result = selector.GetUnixPaths(spec);

            Assert.Null(result);
        }

        [Fact]
        public void IsCurrentPlatformUnixMatchesPlatform()
        {
            var selector = new PlatformPathSelector();

            Assert.Equal(
                Platform.CurrentPlatformIsUnix,
                selector.IsCurrentPlatformUnix);
        }
    }
}
