using System.Collections.ObjectModel;
using Cnct.Core.Configuration;
using Cnct.Core.Validation;

namespace Cnct.Core.Tests
{
    public class CloneGitRepositoryTaskSpecificationTests
    {
        [Fact]
        public void CanDeserializeCloneGitRepositoryTaskSpec()
        {
            var expectedRepos = new Dictionary<string, string>
            {
                ["https://example.com/org/repo-a"] = "~/dev/repo-a",
            };

            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "cloneGitRepository",
                ["repos"] = expectedRepos,
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<CloneGitRepositoryTaskSpecification>(specInterface);
            Assert.Equal(expectedRepos.Count, spec.Repos.Count);
            Assert.Equal(expectedRepos["https://example.com/org/repo-a"], spec.Repos["https://example.com/org/repo-a"]);
        }

        [Fact]
        public void CanDeserializeCloneGitRepositoryTaskSpecWithMultipleRepos()
        {
            var expectedRepos = new Dictionary<string, string>
            {
                ["https://example.com/org/repo-a"] = "~/dev/repo-a",
                ["https://example.com/org/repo-b"] = "~/dev/repo-b",
            };

            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "cloneGitRepository",
                ["repos"] = expectedRepos,
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<CloneGitRepositoryTaskSpecification>(specInterface);
            Assert.Equal(expectedRepos.Count, spec.Repos.Count);
            foreach (var kvp in expectedRepos)
            {
                Assert.Equal(kvp.Value, spec.Repos[kvp.Key]);
            }
        }

        [Fact]
        public void Validate_ReturnsError_WhenReposIsNull()
        {
            var spec = new CloneGitRepositoryTaskSpecification { Repos = null };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void Validate_ReturnsError_WhenReposIsEmpty()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>(),
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void Validate_ReturnsNoErrors_WhenSpecIsValid()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://example.com/org/repo-a"] = "~/dev/repo-a",
                },
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.DoesNotContain(issues, i => i.Severity == ValidationSeverity.Error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ReturnsError_WhenEntryValueIsNullOrWhiteSpace(string dest)
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://example.com/org/repo-a"] = dest,
                },
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void GetDisplayText_SingleRepo_ReturnsActionTypeAndUri()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>
                    {
                        ["https://github.com/user/repo"] = "~/repos/repo",
                    }),
            };

            Assert.Equal("cloneGitRepository: https://github.com/user/repo", spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_MultipleRepos_ReturnsAllUris()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>
                    {
                        ["https://github.com/user/repo1"] = "~/repos/repo1",
                        ["https://github.com/user/repo2"] = "~/repos/repo2",
                    }),
            };

            Assert.Equal(
                "cloneGitRepository: https://github.com/user/repo1, https://github.com/user/repo2",
                spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_NullRepos_ReturnsActionType()
        {
            var spec = new CloneGitRepositoryTaskSpecification();

            Assert.Equal("cloneGitRepository", spec.GetDisplayText());
        }

        [Fact]
        public void Validate_ReturnsNoIssues_WhenAllUrlsAreValidAbsoluteUris()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://github.com/user/repo"] = "~/dev/repo",
                    ["ssh://git@github.com/user/repo.git"] = "~/dev/repo2",
                },
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.Empty(issues);
        }

        [Fact]
        public void Validate_ReturnsError_WhenUrlIsNotAValidAbsoluteUri()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["not a valid url"] = "~/dev/repo",
                },
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.Single(issues);
            Assert.Equal(ValidationSeverity.Error, issues[0].Severity);
            Assert.Contains("not a valid absolute URI", issues[0].Message);
        }
    }
}
