using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
using Cnct.Core.Validation;
using Moq;
using Newtonsoft.Json;
using Xunit;

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

            var json = JsonConvert.SerializeObject(
                new Dictionary<string, object>
                {
                    ["actionType"] = "cloneGitRepository",
                    ["repos"] = expectedRepos,
                });

            var specInterface =
                JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec =
                Assert.IsType<CloneGitRepositoryTaskSpecification>(
                    specInterface);
            Assert.Equal(expectedRepos.Count, spec.Repos.Count);
            Assert.Equal(
                expectedRepos["https://example.com/org/repo-a"],
                spec.Repos["https://example.com/org/repo-a"]);
        }

        [Fact]
        public void CanDeserializeWithMultipleRepos()
        {
            var expectedRepos = new Dictionary<string, string>
            {
                ["https://example.com/org/repo-a"] = "~/dev/repo-a",
                ["https://example.com/org/repo-b"] = "~/dev/repo-b",
            };

            var json = JsonConvert.SerializeObject(
                new Dictionary<string, object>
                {
                    ["actionType"] = "cloneGitRepository",
                    ["repos"] = expectedRepos,
                });

            var specInterface =
                JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec =
                Assert.IsType<CloneGitRepositoryTaskSpecification>(
                    specInterface);
            Assert.Equal(expectedRepos.Count, spec.Repos.Count);
            foreach (var kvp in expectedRepos)
            {
                Assert.Equal(kvp.Value, spec.Repos[kvp.Key]);
            }
        }

        [Fact]
        public void Validate_ReturnsError_WhenReposIsNull()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = null,
            };

            IReadOnlyList<ValidationIssue> issues =
                spec.Validate("/config");

            Assert.Contains(
                issues,
                i => i.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void Validate_ReturnsError_WhenReposIsEmpty()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>(),
            };

            IReadOnlyList<ValidationIssue> issues =
                spec.Validate("/config");

            Assert.Contains(
                issues,
                i => i.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void Validate_ReturnsNoErrors_WhenSpecIsValid()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://example.com/org/repo-a"] =
                        "~/dev/repo-a",
                },
            };

            IReadOnlyList<ValidationIssue> issues =
                spec.Validate("/config");

            Assert.DoesNotContain(
                issues,
                i => i.Severity == ValidationSeverity.Error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ReturnsError_WhenDestIsBlank(
            string dest)
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://example.com/org/repo-a"] = dest,
                },
            };

            IReadOnlyList<ValidationIssue> issues =
                spec.Validate("/config");

            Assert.Contains(
                issues,
                i => i.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public async Task ExecuteAsync_ResolvesRelativeDest()
        {
            string configRoot = Path.GetTempPath();
            const string url =
                "https://example.com/org/repo-a";
            string relativeDest =
                Path.Combine("dev", "repo-a");
            string expectedDest =
                Path.Combine(configRoot, relativeDest);

            var mockFileSystem = new MockFileSystem();
            var mockRunner = new Mock<IGitRunner>();
            mockRunner
                .Setup(r => r.CloneAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var spec = CreateSpec(
                mockRunner, mockFileSystem);
            spec.Repos = new Dictionary<string, string>
            {
                [url] = relativeDest,
            };

            await spec.ExecuteAsync(
                Mock.Of<ILogger>(), configRoot);

            mockRunner.Verify(
                r => r.CloneAsync(url, expectedDest),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ClonesWhenDestNotExist()
        {
            string dest = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString(),
                "repo-a");
            const string url =
                "https://example.com/org/repo-a";

            var mockFileSystem = new MockFileSystem();
            var mockRunner = new Mock<IGitRunner>();
            mockRunner
                .Setup(r => r.CloneAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var spec = CreateSpec(
                mockRunner, mockFileSystem);
            spec.Repos = new Dictionary<string, string>
            {
                [url] = dest,
            };

            await spec.ExecuteAsync(
                Mock.Of<ILogger>(), Path.GetTempPath());

            mockRunner.Verify(
                r => r.CloneAsync(url, dest), Times.Once);
            mockRunner.Verify(
                r => r.PullAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_PullsWhenDotGitDirExists()
        {
            string dest = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString());
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(
                mockFileSystem.Path.Combine(dest, ".git"));

            const string url =
                "https://example.com/org/repo-a";
            var mockRunner = new Mock<IGitRunner>();
            mockRunner
                .Setup(r => r.PullAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var spec = CreateSpec(
                mockRunner, mockFileSystem);
            spec.Repos = new Dictionary<string, string>
            {
                [url] = dest,
            };

            await spec.ExecuteAsync(
                Mock.Of<ILogger>(), Path.GetTempPath());

            mockRunner.Verify(
                r => r.PullAsync(dest), Times.Once);
            mockRunner.Verify(
                r => r.CloneAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_PullsWhenDotGitFileExists()
        {
            string dest = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString());
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(dest);
            mockFileSystem.File.WriteAllText(
                mockFileSystem.Path.Combine(dest, ".git"),
                "gitdir: ../.git/worktrees/worktree1");

            const string url =
                "https://example.com/org/repo-a";
            var mockRunner = new Mock<IGitRunner>();
            mockRunner
                .Setup(r => r.PullAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var spec = CreateSpec(
                mockRunner, mockFileSystem);
            spec.Repos = new Dictionary<string, string>
            {
                [url] = dest,
            };

            await spec.ExecuteAsync(
                Mock.Of<ILogger>(), Path.GetTempPath());

            mockRunner.Verify(
                r => r.PullAsync(dest), Times.Once);
            mockRunner.Verify(
                r => r.CloneAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_LogsWarningWhenNotGitRepo()
        {
            string dest = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString());
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(dest);

            const string url =
                "https://example.com/org/repo-a";
            var mockRunner = new Mock<IGitRunner>();
            var logger = new Mock<ILogger>();

            var spec = CreateSpec(
                mockRunner, mockFileSystem);
            spec.Repos = new Dictionary<string, string>
            {
                [url] = dest,
            };

            await spec.ExecuteAsync(
                logger.Object, Path.GetTempPath());

            logger.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s.Contains(dest))),
                Times.Once);
            mockRunner.Verify(
                r => r.CloneAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
            mockRunner.Verify(
                r => r.PullAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void GetDisplayText_SingleRepo_ReturnsUri()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>
                    {
                        ["https://github.com/user/repo"] =
                            "~/repos/repo",
                    }),
            };

            Assert.Equal(
                "cloneGitRepository: "
                + "https://github.com/user/repo",
                spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_MultipleRepos_ReturnsAll()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>
                    {
                        ["https://github.com/user/repo1"] =
                            "~/repos/repo1",
                        ["https://github.com/user/repo2"] =
                            "~/repos/repo2",
                    }),
            };

            string expected =
                "cloneGitRepository: "
                + "https://github.com/user/repo1, "
                + "https://github.com/user/repo2";
            Assert.Equal(expected, spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_NullRepos_ReturnsActionType()
        {
            var spec = new CloneGitRepositoryTaskSpecification();

            Assert.Equal(
                "cloneGitRepository", spec.GetDisplayText());
        }

        [Fact]
        public void Validate_NoIssues_WhenUrlsAreValidAbsolute()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://github.com/user/repo"] =
                        "~/dev/repo",
                    ["ssh://git@github.com/user/repo.git"] =
                        "~/dev/repo2",
                },
            };

            IReadOnlyList<ValidationIssue> issues =
                spec.Validate("/config");

            Assert.Empty(issues);
        }

        [Fact]
        public void Validate_Error_WhenUrlIsNotAbsoluteUri()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["not a valid url"] = "~/dev/repo",
                },
            };

            IReadOnlyList<ValidationIssue> issues =
                spec.Validate("/config");

            Assert.Single(issues);
            Assert.Equal(
                ValidationSeverity.Error,
                issues[0].Severity);
            Assert.Contains(
                "not a valid absolute URI",
                issues[0].Message);
        }

        private static CloneGitRepositoryTaskSpecification
            CreateSpec(
                Mock<IGitRunner> mockRunner,
                MockFileSystem mockFileSystem)
        {
            var mockFactory = new Mock<IGitRunnerFactory>();
            mockFactory
                .Setup(f => f.Create(It.IsAny<ILogger>()))
                .Returns(mockRunner.Object);

            return new CloneGitRepositoryTaskSpecification(
                mockFactory.Object,
                mockFileSystem,
                new PathResolver(mockFileSystem));
        }
    }
}
