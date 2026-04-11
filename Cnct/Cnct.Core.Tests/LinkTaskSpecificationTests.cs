using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Cnct.Core.Configuration;
using Cnct.Core.Validation;
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

        [Fact]
        public void Validate_ReturnsNoIssues_WhenSourceExists()
        {
            const string configRoot = "/config";
            const string sourceFile = "my-file";
            string fullSourcePath = $"{configRoot}/{sourceFile}";

            var mockFs = new MockFileSystem();
            mockFs.AddFile(fullSourcePath, new MockFileData(string.Empty));

            var spec = new LinkTaskSpecification(new Configuration.FileManagement(), mockFs)
            {
                Links = new Dictionary<string, object> { [sourceFile] = "~/destination" },
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(configRoot);

            Assert.Empty(issues);
        }

        [Fact]
        public void Validate_ReturnsError_WhenSourceDoesNotExist()
        {
            const string configRoot = "/config";
            const string sourceFile = "missing-file";

            var mockFs = new MockFileSystem();

            var spec = new LinkTaskSpecification(new Configuration.FileManagement(), mockFs)
            {
                Links = new Dictionary<string, object> { [sourceFile] = "~/destination" },
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(configRoot);

            Assert.Single(issues);
            Assert.Equal(ValidationSeverity.Error, issues[0].Severity);
            Assert.Contains("does not exist", issues[0].Message);
        }
    }
}
