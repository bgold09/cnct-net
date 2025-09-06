using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cnct.Core.Configuration;
using Xunit;

namespace Cnct.Core.Tests
{
    public class FileManagementTests
    {
        [Fact]
        public void NullFilePath()
        {
            string configRootDirectory = "test";
            string target = "file.ext";
            string expectedFullTargetPath = $"{configRootDirectory}{Path.DirectorySeparatorChar}{target}";

            var fileSpec = new Dictionary<string, object>()
            {
                [target] = null,
            };

            var fileManagement = new FileManagement();

            var actualFiles = fileManagement.GetFileConfigurations(configRootDirectory, fileSpec);

            Assert.Single(actualFiles);
            Assert.Contains(expectedFullTargetPath, actualFiles);

            IEnumerable<string> files = actualFiles[expectedFullTargetPath];
            Assert.Single(files);
            Assert.Equal($"{Platform.Home}{Path.DirectorySeparatorChar}.{target}", files.Single());
        }

        [Fact]
        public void NullFilePathTargetHasPrefixedDot()
        {
            string configRootDirectory = "test";
            string target = ".file.ext";
            string expectedFullTargetPath = $"{configRootDirectory}{Path.DirectorySeparatorChar}{target}";

            var fileSpec = new Dictionary<string, object>()
            {
                [target] = null,
            };

            var fileManagement = new FileManagement();

            var actualFiles = fileManagement.GetFileConfigurations(configRootDirectory, fileSpec);

            Assert.Single(actualFiles);
            Assert.Contains(expectedFullTargetPath, actualFiles);

            IEnumerable<string> files = actualFiles[expectedFullTargetPath];
            Assert.Single(files);
            Assert.Equal($"{Platform.Home}{Path.DirectorySeparatorChar}{target}", files.Single());
        }

        [Fact]
        public void ExplicitFilePath()
        {
            string configRootDirectory = "test";
            string target = "file.ext";
            string expectedFullTargetPath = $"{configRootDirectory}{Path.DirectorySeparatorChar}{target}";

            string explicitFilePath = $"{Path.DirectorySeparatorChar}some{Path.DirectorySeparatorChar}path";

            var fileSpec = new Dictionary<string, object>()
            {
                [target] = explicitFilePath,
            };

            var fileManagement = new FileManagement();

            var actualFiles = fileManagement.GetFileConfigurations(configRootDirectory, fileSpec);

            Assert.Single(actualFiles);
            Assert.Contains(expectedFullTargetPath, actualFiles);

            IEnumerable<string> files = actualFiles[expectedFullTargetPath];
            Assert.Single(files);
            Assert.Equal(explicitFilePath, files.Single());
        }

        /// <summary>
        /// If Windows-specific files are specified, they should only be created in a Windows enviromment.
        /// </summary>
        [Fact]
        public void CreateWindowsFiles()
        {
            TestPlatformFiles(PlatformType.Windows, new FileSpecification
            {
                Windows = Array.Empty<string>(),
            });
        }

        /// <summary>
        /// If Linux-specific files are specified, they should only be created in a Linux environment.
        /// </summary>
        [Fact]
        public void CreateLinuxFiles()
        {
            TestPlatformFiles(PlatformType.Linux, new FileSpecification
            {
                Linux = Array.Empty<string>(),
            });
        }

        /// <summary>
        /// If OSX-specific files are specified, they should only be created in an OSX environment.
        /// </summary>
        [Fact]
        public void CreateOsxFiles()
        {
            TestPlatformFiles(PlatformType.OSX, new FileSpecification
            {
                Osx = Array.Empty<string>(),
            });
        }

        /// <summary>
        /// If UNIX-specific files are specified, they should only be created in UNIX environments (Linux and OSX).
        /// </summary>
        /// <param name="allowedPlatform">The platform that should create files.</param>
        [Fact]
        public void CreateUnixFiles()
        {
            TestPlatformFiles(
                Platform.CurrentPlatformIsUnix,
                new FileSpecification
                {
                    Unix = Array.Empty<string>(),
                });
        }

        private static void TestPlatformFiles(bool predicate, FileSpecification symfileSpec)
        {
            string configRootDirectory = "test";
            string target = "file.ext";
            string expectedFullTargetPath = $"{configRootDirectory}{Path.DirectorySeparatorChar}{target}";
            var fileSpec = new Dictionary<string, object>()
            {
                [target] = symfileSpec,
            };

            var fileManagement = new FileManagement();

            var actualFiles = fileManagement.GetFileConfigurations(configRootDirectory, fileSpec);

            if (predicate)
            {
                Assert.Single(actualFiles);
                Assert.Contains(expectedFullTargetPath, actualFiles);

                IEnumerable<string> files = actualFiles[expectedFullTargetPath];
                Assert.Single(files);
                Assert.Equal($"{Platform.Home}{Path.DirectorySeparatorChar}.{target}", files.Single());
            }
            else
            {
                Assert.Empty(actualFiles);
            }
        }

        private static void TestPlatformFiles(PlatformType allowedPlatform, FileSpecification symfileSpec)
        {
            TestPlatformFiles(allowedPlatform == Platform.CurrentPlatform, symfileSpec);
        }
    }
}
