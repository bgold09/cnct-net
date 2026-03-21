using System;
using System.Collections.Generic;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class LinkExpandTaskSpecificationTests
    {
        [Fact]
        public void CanDeserializeLinkExpandTaskSpec()
        {
            const string source = "~/dev/scripts-pr/skills";
            const string target = "~/.github/skills";

            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "linkExpand",
                ["source"] = source,
                ["target"] = target,
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<LinkExpandTaskSpecification>(specInterface);
            Assert.Equal(source, spec.Source);
            Assert.Equal(target, spec.Target);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ThrowsWhenSourceIsNullOrWhiteSpace(string source)
        {
            var spec = new LinkExpandTaskSpecification
            {
                Source = source,
                Target = "~/some/target",
            };

            Assert.Throws<InvalidOperationException>(() => spec.Validate());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ThrowsWhenTargetIsNullOrWhiteSpace(string target)
        {
            var spec = new LinkExpandTaskSpecification
            {
                Source = "~/some/source",
                Target = target,
            };

            Assert.Throws<InvalidOperationException>(() => spec.Validate());
        }

        [Fact]
        public void Validate_DoesNotThrowWithValidSpec()
        {
            var spec = new LinkExpandTaskSpecification
            {
                Source = "~/dev/scripts-pr/skills",
                Target = "~/.github/skills",
            };

            spec.Validate();
        }
    }
}
