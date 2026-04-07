using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Moq;
using Xunit;

namespace Cnct.Core.Tests
{
    public class DisplayTextTests
    {
        [Fact]
        public void GetDisplayText_Shell_ReturnsShellAndCommand()
        {
            var spec = new ShellTaskSpecification
            {
                Shell = ShellTaskSpecification.ShellType.PowerShell,
                Command = "Install-Module foo",
            };

            Assert.Equal("shell: 'PowerShell Install-Module foo'", spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_ShellSh_ReturnsShellAndCommand()
        {
            var spec = new ShellTaskSpecification
            {
                Shell = ShellTaskSpecification.ShellType.Sh,
                Command = "echo hello",
            };

            Assert.Equal("shell: 'Sh echo hello'", spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_Link_ReturnsLink()
        {
            var spec = new LinkTaskSpecification();

            Assert.Equal("link", spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_Copy_ReturnsCopy()
        {
            var spec = new CopyTaskSpecification();

            Assert.Equal("copy", spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_LinkExpand_ReturnsLinkExpand()
        {
            var spec = new LinkExpandTaskSpecification();

            Assert.Equal("linkExpand", spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_EnvironmentVariable_ReturnsNameEqualsValue()
        {
            var spec = new EnvironmentVariableTaskSpecification
            {
                Name = "MY_VAR",
                Value = "hello",
            };

            Assert.Equal("environmentVariable: 'MY_VAR=hello'", spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_CloneGitRepository_SingleRepo_ReturnsGitCloneUri()
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
        public void GetDisplayText_CloneGitRepository_MultipleRepos_ReturnsAllUris()
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
        public void GetDisplayText_CloneGitRepository_NullRepos_ReturnsActionType()
        {
            var spec = new CloneGitRepositoryTaskSpecification();

            Assert.Equal("cloneGitRepository", spec.GetDisplayText());
        }

        [Fact]
        public async Task ExecuteAsync_LogsStartAndFinish_WithDisplayText()
        {
            var logger = new Mock<ILogger>();
            var action = new TestActionSpec();
            var config = new CnctConfig
            {
                Logger = logger.Object,
                ConfigRootDirectory = "/",
                MachineTags = new string[0],
                Actions = new ICnctActionSpec[] { action },
            };

            await config.ExecuteAsync();

            logger.Verify(l => l.LogInformation("> my-action"), Times.Once);
            logger.Verify(
                l => l.LogInformation(It.Is<string>(s => s.StartsWith("✓ my-action"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_UsesLabel_WhenLabelIsSet()
        {
            var logger = new Mock<ILogger>();
            var action = new TestActionSpec { Label = "my custom label" };
            var config = new CnctConfig
            {
                Logger = logger.Object,
                ConfigRootDirectory = "/",
                MachineTags = new string[0],
                Actions = new ICnctActionSpec[] { action },
            };

            await config.ExecuteAsync();

            logger.Verify(l => l.LogInformation("> my-action: my custom label"), Times.Once);
            logger.Verify(
                l => l.LogInformation(It.Is<string>(s => s.StartsWith("✓ my-action: my custom label"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_UsesGetDisplayText_WhenLabelIsNotSet()
        {
            var logger = new Mock<ILogger>();
            var action = new TestActionSpec();
            var config = new CnctConfig
            {
                Logger = logger.Object,
                ConfigRootDirectory = "/",
                MachineTags = new string[0],
                Actions = new ICnctActionSpec[] { action },
            };

            await config.ExecuteAsync();

            logger.Verify(l => l.LogInformation("> my-action"), Times.Once);
        }

        private class TestActionSpec : CnctActionSpecBase
        {
            public override string ActionType => "my-action";

            public override string GetDisplayText() => "my-action";

            public override void Validate()
            {
            }

            public override Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
            {
                return Task.CompletedTask;
            }
        }
    }
}
