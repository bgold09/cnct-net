using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
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
            var (config, runner) = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAction_WhenMachineHasTags()
        {
            var action = new TestActionSpec();
            var (config, runner) = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_WhenMachineTagMatches()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var (config, runner) = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsTaggedAction_WhenNoMachineTags()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var (config, runner) = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ICnctActionSpec>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsTaggedAction_WhenMachineTagsDoNotMatch()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var (config, runner) = MakeConfig(new[] { "work" }, action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ICnctActionSpec>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_CaseInsensitiveMatch()
        {
            var action = new TestActionSpec { Tags = new[] { "Personal" } };
            var (config, runner) = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_WhenAnyTagMatches()
        {
            var action = new TestActionSpec { Tags = new[] { "personal", "home" } };
            var (config, runner) = MakeConfig(new[] { "work", "home" }, action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAndSkipsTagged_Mixed()
        {
            var untaggedAction = new TestActionSpec();
            var taggedAction = new TestActionSpec { Tags = new[] { "personal" } };
            var (config, runner) = MakeConfig(
                Array.Empty<string>(), untaggedAction, taggedAction);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(
                    untaggedAction, It.IsAny<ILogger>(), "/"),
                Times.Once);
            runner.Verify(
                r => r.ExecuteAsync(
                    taggedAction, It.IsAny<ILogger>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_LogsStartAndFinish_WithDisplayText()
        {
            var logger = new Mock<ILogger>();
            var runner = new Mock<IActionRunner>();
            var action = new TestActionSpec();
            var config = new CnctConfig
            {
                Logger = logger.Object,
                Runner = runner.Object,
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
            var runner = new Mock<IActionRunner>();
            var action = new TestActionSpec { Label = "my custom label" };
            var config = new CnctConfig
            {
                Logger = logger.Object,
                Runner = runner.Object,
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
            var runner = new Mock<IActionRunner>();
            var action = new TestActionSpec();
            var config = new CnctConfig
            {
                Logger = logger.Object,
                Runner = runner.Object,
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
            var runner = new Mock<IActionRunner>();
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
                Runner = runner.Object,
                ConfigRootDirectory = "/",
                MachineTags = Array.Empty<string>(),
                Actions = new ICnctActionSpec[] { action },
            };

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ICnctActionSpec>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>()),
                Times.Never);
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
            var (config, runner) = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsAction_WhenOsNotSpecified()
        {
            var action = new TestActionSpec();
            var (config, runner) = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            runner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
            Assert.Null(action.PlatformType);
        }

        private static (CnctConfig Config, Mock<IActionRunner> Runner) MakeConfig(
            IReadOnlyCollection<string> machineTags,
            params ICnctActionSpec[] actions)
        {
            var runner = new Mock<IActionRunner>();
            var config = new CnctConfig
            {
                Logger = Mock.Of<ILogger>(),
                Runner = runner.Object,
                ConfigRootDirectory = "/",
                MachineTags = machineTags,
                Actions = actions,
            };

            return (config, runner);
        }

        private class TestActionSpec : CnctActionSpecBase
        {
            public override string ActionType => "test";
        }
    }
}
