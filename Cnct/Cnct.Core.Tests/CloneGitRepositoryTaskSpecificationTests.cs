using System;
using System.Collections.Generic;
using System.IO;
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

            var mockRunner = new Mock<IGitRunner>();
            mockRunner.Setup(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var logger = Mock.Of<ILogger>();
            var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object)
            {
                Repos = new Dictionary<string, string> { [url] = relativeDest },
            };

            await spec.ExecuteAsync(logger, configRoot);

            mockRunner.Verify(
                r => r.CloneAsync(url, expectedDest),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ClonesWhenDestDoesNotExist()
        {
            string dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "repo-a");
            const string url = "https://example.com/org/repo-a";

            var mockRunner = new Mock<IGitRunner>();
            mockRunner.Setup(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object)
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
            Directory.CreateDirectory(Path.Combine(dest, ".git"));
            try
            {
                const string url = "https://example.com/org/repo-a";
                var mockRunner = new Mock<IGitRunner>();
                mockRunner.Setup(r => r.PullAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

                var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object)
                {
                    Repos = new Dictionary<string, string> { [url] = dest },
                };

                await spec.ExecuteAsync(Mock.Of<ILogger>(), Path.GetTempPath());

                mockRunner.Verify(r => r.PullAsync(dest), Times.Once);
                mockRunner.Verify(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
            finally
            {
                Directory.Delete(dest, recursive: true);
            }
        }

        [Fact]
        public async Task ExecuteAsync_PullsWhenDotGitFileExists()
        {
            string dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dest);
            File.WriteAllText(Path.Combine(dest, ".git"), "gitdir: ../.git/worktrees/worktree1");
            try
            {
                const string url = "https://example.com/org/repo-a";
                var mockRunner = new Mock<IGitRunner>();
                mockRunner.Setup(r => r.PullAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

                var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object)
                {
                    Repos = new Dictionary<string, string> { [url] = dest },
                };

                await spec.ExecuteAsync(Mock.Of<ILogger>(), Path.GetTempPath());

                mockRunner.Verify(r => r.PullAsync(dest), Times.Once);
                mockRunner.Verify(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
            finally
            {
                Directory.Delete(dest, recursive: true);
            }
        }

        [Fact]
        public async Task ExecuteAsync_LogsWarningWhenDestExistsButIsNotGitRepo()
        {
            string dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dest);
            try
            {
                const string url = "https://example.com/org/repo-a";
                var mockRunner = new Mock<IGitRunner>();
                var logger = new Mock<ILogger>();

                var spec = new CloneGitRepositoryTaskSpecification(mockRunner.Object)
                {
                    Repos = new Dictionary<string, string> { [url] = dest },
                };

                await spec.ExecuteAsync(logger.Object, Path.GetTempPath());

                logger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains(dest))), Times.Once);
                mockRunner.Verify(r => r.CloneAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                mockRunner.Verify(r => r.PullAsync(It.IsAny<string>()), Times.Never);
            }
            finally
            {
                Directory.Delete(dest, recursive: true);
            }
        }
    }
}
