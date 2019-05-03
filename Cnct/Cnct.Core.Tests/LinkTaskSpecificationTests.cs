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
                    { target, null }
                }
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
                    { target, null }
                }
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
                    { target, explicitLinkPath }
                }
            };

            var actualLinks = linkSpec.GetLinkConfigurations(configRootDirectory);

            Assert.Equal(1, actualLinks.Count);
            Assert.Contains(expectedFullTargetPath, actualLinks);

            IEnumerable<string> links = actualLinks[expectedFullTargetPath];
            Assert.Single(links);
            Assert.Equal(explicitLinkPath, links.Single());
        }
    }
}
