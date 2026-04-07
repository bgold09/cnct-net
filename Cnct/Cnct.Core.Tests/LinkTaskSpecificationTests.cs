using System.Collections.Generic;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class LinkTaskSpecificationTests
    {
        [Fact]
        public void CanDeserializeLinkTaskSpec()
        {
            var expectedLinks = new Dictionary<string, string>
            {
                ["file"] = "destination",
            };

            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "link",
                ["links"] = expectedLinks,
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<LinkTaskSpecification>(specInterface);
            Assert.Equal(expectedLinks.Count, spec.Links.Count);
        }
        [Fact]
        public void GetDisplayText_ReturnsLink()
        {
            var spec = new LinkTaskSpecification();

            Assert.Equal("link", spec.GetDisplayText());
        }
    }
}
