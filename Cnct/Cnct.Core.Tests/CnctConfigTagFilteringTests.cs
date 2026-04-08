using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Moq;
using Xunit;

namespace Cnct.Core.Tests
{
    public class CnctConfigTagFilteringTests
    {
        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAction_WhenMachineHasNoTags()
        {
            var action = new TestActionSpec();
            var config = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAction_WhenMachineHasTags()
        {
            var action = new TestActionSpec();
            var config = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_WhenMachineTagMatches()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var config = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsTaggedAction_WhenNoMachineTags()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var config = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            Assert.False(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsTaggedAction_WhenMachineTagsDoNotMatch()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var config = MakeConfig(new[] { "work" }, action);

            await config.ExecuteAsync();

            Assert.False(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_CaseInsensitiveMatch()
        {
            var action = new TestActionSpec { Tags = new[] { "Personal" } };
            var config = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_WhenAnyTagMatches()
        {
            var action = new TestActionSpec { Tags = new[] { "personal", "home" } };
            var config = MakeConfig(new[] { "work", "home" }, action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAndSkipsTagged_Mixed()
        {
            var untaggedAction = new TestActionSpec();
            var taggedAction = new TestActionSpec { Tags = new[] { "personal" } };
            var config = MakeConfig(Array.Empty<string>(), untaggedAction, taggedAction);

            await config.ExecuteAsync();

            Assert.True(untaggedAction.WasExecuted);
            Assert.False(taggedAction.WasExecuted);
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
                MachineTags = Array.Empty<string>(),
                Actions = new ICnctActionSpec[] { action },
            };

            await config.ExecuteAsync();

            logger.Verify(l => l.LogStart("test"), Times.Once);
            logger.Verify(
                l => l.LogFinish(It.Is<string>(s => s.StartsWith("test"))),
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
                MachineTags = Array.Empty<string>(),
                Actions = new ICnctActionSpec[] { action },
            };

            await config.ExecuteAsync();

            logger.Verify(l => l.LogStart("test: my custom label"), Times.Once);
            logger.Verify(
                l => l.LogFinish(It.Is<string>(s => s.StartsWith("test: my custom label"))),
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
                MachineTags = Array.Empty<string>(),
                Actions = new ICnctActionSpec[] { action },
            };

            await config.ExecuteAsync();

            logger.Verify(l => l.LogStart("test"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsAction_WhenOsDoesNotMatch()
        {
            var logger = new Mock<ILogger>();
            var nonCurrentPlatform = Platform.CurrentPlatform == PlatformType.Windows
                ? PlatformType.Linux
                : PlatformType.Windows;
            var action = new TestActionSpec
            {
                PlatformType = new[] { nonCurrentPlatform },
            };
            var config = new CnctConfig
            {
                Logger = logger.Object,
                ConfigRootDirectory = "/",
                MachineTags = Array.Empty<string>(),
                Actions = new ICnctActionSpec[] { action },
            };

            await config.ExecuteAsync();

            Assert.False(action.WasExecuted);
            logger.Verify(
                l => l.LogStart(It.IsAny<string>()),
                Times.Never);
            logger.Verify(
                l => l.LogFinish(It.IsAny<string>()),
                Times.Never);
            logger.Verify(
                l => l.LogVerbose(
                    It.Is<string>(
                        s => s.Contains("not applicable to current OS"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsAction_WhenOsMatches()
        {
            var action = new TestActionSpec
            {
                PlatformType = new[] { Platform.CurrentPlatform },
            };
            var config = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsAction_WhenOsNotSpecified()
        {
            var action = new TestActionSpec();
            var config = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
            Assert.Null(action.PlatformType);
        }

        private static CnctConfig MakeConfig(IReadOnlyCollection<string> machineTags, params ICnctActionSpec[] actions)
        {
            return new CnctConfig
            {
                Logger = Mock.Of<ILogger>(),
                ConfigRootDirectory = "/",
                MachineTags = machineTags,
                Actions = actions,
            };
        }

        private class TestActionSpec : CnctActionSpecBase
        {
            public bool WasExecuted { get; private set; }

            public override string ActionType => "test";

            public override void Validate()
            {
            }

            public override Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
            {
                this.WasExecuted = true;
                return Task.CompletedTask;
            }
        }
    }
}
