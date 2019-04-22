using System;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class SymlinkSpecificationTests
    {
        [Fact]
        public void CanDeserializeAllPlatformLinks()
        {
            string json = @"{
  ""windows"": null,
  ""osx"": null,
  ""linux"": null,
}";

            var s = JsonConvert.DeserializeObject<SymlinkSpecification>(json);
            Assert.Equal(Array.Empty<string>(), s.Windows);
            Assert.Equal(Array.Empty<string>(), s.Linux);
            Assert.Equal(Array.Empty<string>(), s.Osx);
        }

        [Fact]
        public void CanDeserializeSinglePlatformLinks()
        {
            string json = @"{
  ""windows"": null
}";

            var s = JsonConvert.DeserializeObject<SymlinkSpecification>(json);
            Assert.Equal(Array.Empty<string>(), s.Windows);
            Assert.Null(s.Linux);
            Assert.Null(s.Osx);
        }

        [Fact]
        public void CanDeserializeWithExplicitLinkPath()
        {
            string expectedLink = "some/path";
            string json = @"{
  ""windows"": """ + expectedLink + "\"}";

            var s = JsonConvert.DeserializeObject<SymlinkSpecification>(json);
            Assert.Equal(new[] { expectedLink }, s.Windows);
            Assert.Null(s.Linux);
            Assert.Null(s.Osx);
        }
    }
}
