using System.IO;
using Cnct.Core.Configuration;
using Xunit;

namespace Cnct.Core.Tests
{
    public class PathExtensionsTests
    {
        [Fact]
        public void NormalizePathReplcacesHomeTilde()
        {
            var pathSegment = "/some/path/file.config";
            var unnormalizedPath = $"~{pathSegment}";

            string actualNormalizedPath = PathExtensions.NormalizePath(unnormalizedPath);

            Assert.Equal(
                $"{Platform.Home}{pathSegment.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)}",
                actualNormalizedPath);
        }
    }
}
