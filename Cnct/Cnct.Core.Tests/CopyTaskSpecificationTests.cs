using Cnct.Core.Configuration;
using Cnct.Core.Validation;

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

        [Fact]
        public void Validate_ReturnsNoIssues_WhenSourceExists()
        {
            const string configRoot = "/config";
            const string sourceFile = "my-file";
            string fullSourcePath = $"{configRoot}/{sourceFile}";

            var mockFs = new MockFileSystem();
            mockFs.AddFile(fullSourcePath, new MockFileData(string.Empty));

            var spec = new CopyTaskSpecification(new Configuration.FileManagement(), mockFs)
            {
                Files = new Dictionary<string, object> { [sourceFile] = "~/destination" },
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext(configRoot, []));

            Assert.Empty(issues);
        }

        [Fact]
        public void Validate_ReturnsError_WhenSourceDoesNotExist()
        {
            const string configRoot = "/config";
            const string sourceFile = "missing-file";

            var mockFs = new MockFileSystem();

            var spec = new CopyTaskSpecification(new Configuration.FileManagement(), mockFs)
            {
                Files = new Dictionary<string, object> { [sourceFile] = "~/destination" },
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext(configRoot, []));

            Assert.Single(issues);
            Assert.Equal(ValidationSeverity.Error, issues[0].Severity);
            Assert.Contains("does not exist", issues[0].Message);
        }

        [Fact]
        public void Validate_SkipsSourceExistenceCheck_WhenActionTagsDoNotMatchMachineTags()
        {
            var mockFs = new MockFileSystem();
            var spec = new CopyTaskSpecification(new Configuration.FileManagement(), mockFs)
            {
                Files = new Dictionary<string, object> { ["missing-file"] = "~/destination" },
                Tags = ["work"],
            };

            IReadOnlyList<ValidationIssue> issues =
                spec.Validate(new CnctContext("/config", ["personal"]));

            Assert.Empty(issues);
        }

        [Fact]
        public void Validate_SkipsSourceExistenceCheck_WhenPlatformDoesNotMatch()
        {
            var mockFs = new MockFileSystem();
            PlatformType otherPlatform = Platform.CurrentPlatform == PlatformType.Windows
                ? PlatformType.Linux
                : PlatformType.Windows;

            var spec = new CopyTaskSpecification(new Configuration.FileManagement(), mockFs)
            {
                Files = new Dictionary<string, object> { ["missing-file"] = "~/destination" },
                PlatformType = [otherPlatform],
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.Empty(issues);
        }

        [Fact]
        public void Validate_StillReportsEmptyFiles_WhenActionWouldBeSkipped()
        {
            var mockFs = new MockFileSystem();
            var spec = new CopyTaskSpecification(new Configuration.FileManagement(), mockFs)
            {
                Files = null,
                Tags = ["work"],
            };

            IReadOnlyList<ValidationIssue> issues =
                spec.Validate(new CnctContext("/config", ["personal"]));

            Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error);
        }
    }
}
