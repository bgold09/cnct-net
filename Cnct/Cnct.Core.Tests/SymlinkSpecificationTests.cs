using Cnct.Core.Configuration;

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

            var s = JsonConvert.DeserializeObject<FileSpecification>(json);
            Assert.Equal((string[])[], s.Windows);
            Assert.Equal((string[])[], s.Linux);
            Assert.Equal((string[])[], s.Osx);
        }

        [Fact]
        public void CanDeserializeSinglePlatformLinks()
        {
            string json = @"{
  ""windows"": null
}";

            var s = JsonConvert.DeserializeObject<FileSpecification>(json);
            Assert.Equal((string[])[], s.Windows);
            Assert.Null(s.Linux);
            Assert.Null(s.Osx);
        }

        [Fact]
        public void CanDeserializeWithExplicitLinkPath()
        {
            string expectedLink = "some/path";
            string json = @"{
      ""windows"": """ + expectedLink + "\"}";

            var s = JsonConvert.DeserializeObject<FileSpecification>(json);
            Assert.Equal([expectedLink], s.Windows);
            Assert.Null(s.Linux);
            Assert.Null(s.Osx);
        }
    }
}
