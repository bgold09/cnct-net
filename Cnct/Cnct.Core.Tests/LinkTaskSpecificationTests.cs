using System.Collections.Generic;
using System.Linq;
using Cnct.Core.Configuration;
using Xunit;

namespace Cnct.Core.Tests
{
    public class LinkTaskSpecificationTests
    {
        [Fact]
        public void ExplicitLinkPath()
        {
            string target = "file.ext";
            string explicitLinkPath = "~/some/pathrc";

            var linkSpec = new LinkTaskSpecification()
            {
                Links = new Dictionary<string, object>()
                {
                    { target, explicitLinkPath }
                }
            };

            var actualLinks = linkSpec.GetLinkConfigurations();

            Assert.Equal(1, actualLinks.Count);
            Assert.Contains(target, actualLinks);

            IEnumerable<string> links = actualLinks[target];
            Assert.Single(links);
            Assert.Equal(explicitLinkPath, links.Single());
        }
    }
}
