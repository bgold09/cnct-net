using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
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
        public void Validate_ThrowsWhenReposIsNull()
        {
            var spec = new CloneGitRepositoryTaskSpecification { Repos = null };
            Assert.Throws<InvalidOperationException>(() => spec.Validate());
        }

        [Fact]
        public void Validate_ThrowsWhenReposIsEmpty()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>(),
            };

            Assert.Throws<InvalidOperationException>(() => spec.Validate());
        }

        [Fact]
        public void Validate_DoesNotThrowWithValidSpec()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://example.com/org/repo-a"] = "~/dev/repo-a",
                },
            };

            spec.Validate();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ThrowsWhenEntryValueIsNullOrWhiteSpace(string dest)
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://example.com/org/repo-a"] = dest,
                },
            };

            Assert.Throws<InvalidOperationException>(() => spec.Validate());
        }

        [Fact]
        public async Task ExecuteAsync_ResolvesRelativeDestAgainstConfigDirectoryRoot()
        {
            string configRoot = Path.GetTempPath();
            const string url = "https://example.com/org/repo-a";
            string relativeDest = Path.Combine("dev", "repo-a");
            string expectedDest = Path.Combine(configRoot, relativeDest);

            var mockFileSystem = new MockFileSystem();
            var mockRunner = new Mock<IGitRunner>();
            mockRunner.Setup(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object, mockFileSystem)
            {
                Repos = new Dictionary<string, string> { [url] = relativeDest },
            };

            await spec.ExecuteAsync(Mock.Of<ILogger>(), configRoot);

            mockRunner.Verify(
                r => r.CloneAsync(url, expectedDest),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ClonesWhenDestDoesNotExist()
        {
            string dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "repo-a");
            const string url = "https://example.com/org/repo-a";

            var mockFileSystem = new MockFileSystem();
            var mockRunner = new Mock<IGitRunner>();
            mockRunner.Setup(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object, mockFileSystem)
            {
                Repos = new Dictionary<string, string> { [url] = dest },
            };

            await spec.ExecuteAsync(Mock.Of<ILogger>(), Path.GetTempPath());

            mockRunner.Verify(r => r.CloneAsync(url, dest), Times.Once);
            mockRunner.Verify(r => r.PullAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_PullsWhenDotGitDirectoryExists()
        {
            string dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(mockFileSystem.Path.Combine(dest, ".git"));

            const string url = "https://example.com/org/repo-a";
            var mockRunner = new Mock<IGitRunner>();
            mockRunner.Setup(r => r.PullAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object, mockFileSystem)
            {
                Repos = new Dictionary<string, string> { [url] = dest },
            };

            await spec.ExecuteAsync(Mock.Of<ILogger>(), Path.GetTempPath());

            mockRunner.Verify(r => r.PullAsync(dest), Times.Once);
            mockRunner.Verify(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_PullsWhenDotGitFileExists()
        {
            string dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(dest);
            mockFileSystem.File.WriteAllText(
                mockFileSystem.Path.Combine(dest, ".git"),
                "gitdir: ../.git/worktrees/worktree1");

            const string url = "https://example.com/org/repo-a";
            var mockRunner = new Mock<IGitRunner>();
            mockRunner.Setup(r => r.PullAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object, mockFileSystem)
            {
                Repos = new Dictionary<string, string> { [url] = dest },
            };

            await spec.ExecuteAsync(Mock.Of<ILogger>(), Path.GetTempPath());

            mockRunner.Verify(r => r.PullAsync(dest), Times.Once);
            mockRunner.Verify(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_LogsWarningWhenDestExistsButIsNotGitRepo()
        {
            string dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(dest);

            const string url = "https://example.com/org/repo-a";
            var mockRunner = new Mock<IGitRunner>();
            var logger = new Mock<ILogger>();

            var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object, mockFileSystem)
            {
                Repos = new Dictionary<string, string> { [url] = dest },
            };

            await spec.ExecuteAsync(logger.Object, Path.GetTempPath());

            logger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains(dest))), Times.Once);
            mockRunner.Verify(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            mockRunner.Verify(r => r.PullAsync(It.IsAny<string>()), Times.Never);
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
    }
}
