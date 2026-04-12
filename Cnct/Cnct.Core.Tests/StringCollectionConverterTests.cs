using Cnct.Core.Configuration;

namespace Cnct.Core.Tests
{
    public class StringCollectionConverterTests
    {
        [Fact]
        public void CanDeserializeSingleString()
        {
            var json = JsonConvert.SerializeObject(new { tags = "personal" });
            var result = JsonConvert.DeserializeObject<TagsWrapper>(json);
            Assert.Equal(["personal"], result.Tags);
        }

        [Fact]
        public void CanDeserializeStringArray()
        {
            var json = JsonConvert.SerializeObject(new { tags = new[] { "personal", "home" } });
            var result = JsonConvert.DeserializeObject<TagsWrapper>(json);
            Assert.Equal(2, result.Tags.Count);
            Assert.Contains("personal", result.Tags);
            Assert.Contains("home", result.Tags);
        }

        [Fact]
        public void CanDeserializeNullAsEmptyCollection()
        {
            var json = "{\"tags\":null}";
            var result = JsonConvert.DeserializeObject<TagsWrapper>(json);
            Assert.Empty(result.Tags);
        }

        [Fact]
        public void CanDeserializeEmptyArray()
        {
            var json = JsonConvert.SerializeObject(new { tags = new string[0] });
            var result = JsonConvert.DeserializeObject<TagsWrapper>(json);
            Assert.Empty(result.Tags);
        }

        [Fact]
        public void DefaultsToEmptyCollectionWhenPropertyAbsent()
        {
            var json = "{}";
            var result = JsonConvert.DeserializeObject<TagsWrapper>(json);
            Assert.Empty(result.Tags);
        }

        private class TagsWrapper
        {
            [JsonProperty("tags")]
            [JsonConverter(typeof(StringCollectionConverter))]
            public IReadOnlyCollection<string> Tags { get; set; } = [];
        }
    }
}
