using Cnct.Core.Configuration;
using Cnct.Core.Validation;

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
        public void Validate_ReturnsError_WhenSourceIsNullOrWhiteSpace(string source)
        {
            var mockFs = new MockFileSystem();
            var spec = new LinkExpandTaskSpecification(mockFs, new PathResolver(mockFs))
            {
                Source = source,
                Target = "~/some/target",
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate("/config");

            Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error && i.Message.Contains("source"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ReturnsError_WhenTargetIsNullOrWhiteSpace(string target)
        {
            var mockFs = new MockFileSystem();
            var spec = new LinkExpandTaskSpecification(mockFs, new PathResolver(mockFs))
            {
                Source = "~/some/source",
                Target = target,
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate("/config");

            Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error && i.Message.Contains("target"));
        }

        [Fact]
        public void GetDisplayText_ReturnsLinkExpand()
        {
            var spec = new LinkExpandTaskSpecification();

            Assert.Equal("linkExpand", spec.GetDisplayText());
        }

        [Fact]
        public void Validate_ReturnsNoIssues_WhenSourceDirectoryExists()
        {
            const string configRoot = "/config";
            const string source = "my-scripts";
            string fullSourcePath = $"{configRoot}/{source}";

            var mockFs = new MockFileSystem();
            mockFs.AddDirectory(fullSourcePath);

            var spec = new LinkExpandTaskSpecification(mockFs, new PathResolver(mockFs))
            {
                Source = source,
                Target = "~/some/target",
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(configRoot);

            Assert.Empty(issues);
        }

        [Fact]
        public void Validate_ReturnsError_WhenSourceDirectoryDoesNotExist()
        {
            const string configRoot = "/config";
            const string source = "missing-scripts";

            var mockFs = new MockFileSystem();

            var spec = new LinkExpandTaskSpecification(mockFs, new PathResolver(mockFs))
            {
                Source = source,
                Target = "~/some/target",
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(configRoot);

            Assert.Single(issues);
            Assert.Equal(ValidationSeverity.Error, issues[0].Severity);
            Assert.Contains("does not exist", issues[0].Message);
        }
    }
}
