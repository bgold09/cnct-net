using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cnct.Core.Configuration;
using Xunit;

namespace Cnct.Core.Tests
{
    public class LinkTaskSpecificationTests
    {
        [Fact]
        public void NullLinkPath()
        {
            string configRootDirectory = "test";
            string target = "file.ext";
            string expectedFullTargetPath = $"{configRootDirectory}{Path.DirectorySeparatorChar}{target}";

            var linkSpec = new LinkTaskSpecification()
            {
                Links = new Dictionary<string, object>()
                {
                    { target, null },
                },
            };

            var actualLinks = linkSpec.GetLinkConfigurations(configRootDirectory);

            Assert.Equal(1, actualLinks.Count);
            Assert.Contains(expectedFullTargetPath, actualLinks);

            IEnumerable<string> links = actualLinks[expectedFullTargetPath];
            Assert.Single(links);
            Assert.Equal($"{Platform.Home}{Path.DirectorySeparatorChar}.{target}", links.Single());
        }

        [Fact]
        public void NullLinkPathTargetHasPrefixedDot()
        {
            string configRootDirectory = "test";
            string target = ".file.ext";
            string expectedFullTargetPath = $"{configRootDirectory}{Path.DirectorySeparatorChar}{target}";

            var linkSpec = new LinkTaskSpecification()
            {
                Links = new Dictionary<string, object>()
                {
                    { target, null },
                },
            };

            var actualLinks = linkSpec.GetLinkConfigurations(configRootDirectory);

            Assert.Equal(1, actualLinks.Count);
            Assert.Contains(expectedFullTargetPath, actualLinks);

            IEnumerable<string> links = actualLinks[expectedFullTargetPath];
            Assert.Single(links);
            Assert.Equal($"{Platform.Home}{Path.DirectorySeparatorChar}{target}", links.Single());
        }

        [Fact]
        public void ExplicitLinkPath()
        {
            string configRootDirectory = "test";
            string target = "file.ext";
            string expectedFullTargetPath = $"{configRootDirectory}{Path.DirectorySeparatorChar}{target}";

            string explicitLinkPath = $"{Path.DirectorySeparatorChar}some{Path.DirectorySeparatorChar}path";

            var linkSpec = new LinkTaskSpecification()
            {
                Links = new Dictionary<string, object>()
                {
                    { target, explicitLinkPath },
                },
            };

            var actualLinks = linkSpec.GetLinkConfigurations(configRootDirectory);

            Assert.Equal(1, actualLinks.Count);
            Assert.Contains(expectedFullTargetPath, actualLinks);

            IEnumerable<string> links = actualLinks[expectedFullTargetPath];
            Assert.Single(links);
            Assert.Equal(explicitLinkPath, links.Single());
        }

        /// <summary>
        /// If Windows-specific links are specified, they should only be created in a Windows enviromment.
        /// </summary>
        [Fact]
        public void CreateWindowsLinks()
        {
            TestPlatformLinks(PlatformType.Windows, new SymlinkSpecification
            {
                Windows = Array.Empty<string>(),
            });
        }

        /// <summary>
        /// If Linux-specific links are specified, they should only be created in a Linux environment.
        /// </summary>
        [Fact]
        public void CreateLinuxLinks()
        {
            TestPlatformLinks(PlatformType.Linux, new SymlinkSpecification
            {
                Linux = Array.Empty<string>(),
            });
        }

        /// <summary>
        /// If OSX-specific links are specified, they should only be created in an OSX environment.
        /// </summary>
        [Fact]
        public void CreateOsxLinks()
        {
            TestPlatformLinks(PlatformType.OSX, new SymlinkSpecification
            {
                Osx = Array.Empty<string>(),
            });
        }

        /// <summary>
        /// If UNIX-specific links are specified, they should only be created in UNIX environments (Linux and OSX).
        /// </summary>
        /// <param name="allowedPlatform">The platform that should create links.</param>
        [Theory]
        [InlineData(PlatformType.Linux)]
        [InlineData(PlatformType.OSX)]
        public void CreateUnixLinks(PlatformType allowedPlatform)
        {
            TestPlatformLinks(allowedPlatform, new SymlinkSpecification
            {
                Unix = Array.Empty<string>(),
            });
        }

        private static void TestPlatformLinks(PlatformType allowedPlatform, SymlinkSpecification symlinkSpec)
        {
            string configRootDirectory = "test";
            string target = "file.ext";
            string expectedFullTargetPath = $"{configRootDirectory}{Path.DirectorySeparatorChar}{target}";
            var linkSpec = new LinkTaskSpecification()
            {
                Links = new Dictionary<string, object>()
                {
                    [target] = symlinkSpec,
                },
            };

            var actualLinks = linkSpec.GetLinkConfigurations(configRootDirectory);

            if (allowedPlatform == Platform.CurrentPlatform)
            {
                Assert.Equal(1, actualLinks.Count);
                Assert.Contains(expectedFullTargetPath, actualLinks);

                IEnumerable<string> links = actualLinks[expectedFullTargetPath];
                Assert.Single(links);
                Assert.Equal($"{Platform.Home}{Path.DirectorySeparatorChar}.{target}", links.Single());
            }
            else
            {
                Assert.Empty(actualLinks);
            }
        }
    }
}
