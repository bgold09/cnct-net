using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Cnct.Core.Configuration;
using Xunit;

namespace Cnct.Core.Tests
{
    public class PathResolverTests
    {
        [Fact]
        public void Resolve_AbsolutePath_ReturnedAsIsAfterNormalization()
        {
            var mockFs = new MockFileSystem();
            var resolver = new PathResolver(mockFs);

            string absolutePath = Path.Combine(
                Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar),
                "some",
                "path");

            string result = resolver.Resolve(absolutePath, "/config");

            Assert.Equal(absolutePath, result);
        }

        [Fact]
        public void Resolve_RelativePath_CombinedWithConfigRoot()
        {
            var mockFs = new MockFileSystem();
            var resolver = new PathResolver(mockFs);

            const string configRoot = "/config";
            string relativePath = $"sub{Path.DirectorySeparatorChar}dir";
            string expected = mockFs.Path.Combine(configRoot, relativePath);

            string result = resolver.Resolve(relativePath, configRoot);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Resolve_TildePrefixedPath_ExpandsHomeDirectory()
        {
            var mockFs = new MockFileSystem();
            var resolver = new PathResolver(mockFs);

            string result = resolver.Resolve("~/my/dir", "/config");

            string expected = $"{Platform.Home}{Path.DirectorySeparatorChar}my{Path.DirectorySeparatorChar}dir";
            Assert.Equal(expected, result);
        }
    }
}
