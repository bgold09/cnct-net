using System;
using System.Text.Json;
using Cnct.Core.Configuration;
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
  ""linux"": null
}";

            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IgnoreNullValues = false };
            options.Converters.Add(new SymlinkCollectionConverter());

            var s = JsonSerializer.Deserialize<SymlinkSpecification>(json, options);
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

            var s = JsonSerializer.Deserialize<SymlinkSpecification>(json);
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

            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, };
            options.Converters.Add(new SymlinkCollectionConverter());

            var s = JsonSerializer.Deserialize<SymlinkSpecification>(json, options);
            Assert.Equal(new[] { expectedLink }, s.Windows);
            Assert.Null(s.Linux);
            Assert.Null(s.Osx);
        }
    }
}
