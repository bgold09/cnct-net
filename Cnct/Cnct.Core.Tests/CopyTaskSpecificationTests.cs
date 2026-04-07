using System.Collections.Generic;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class CopyTaskSpecificationTests
    {
        [Fact]
        public void CanDeserializeCopyTaskSpec()
        {
            var expectedFileConfigs = new Dictionary<string, string>
            {
                ["file"] = "destination",
            };

            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "copy",
                ["files"] = expectedFileConfigs,
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<CopyTaskSpecification>(specInterface);
            Assert.Equal(expectedFileConfigs.Count, spec.Files.Count);
        }

        [Fact]
        public void GetDisplayText_ReturnsCopy()
        {
            var spec = new CopyTaskSpecification();

            Assert.Equal("copy", spec.GetDisplayText());
        }
    }
}
