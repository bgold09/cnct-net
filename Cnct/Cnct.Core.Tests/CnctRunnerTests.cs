using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
using Moq;
using Xunit;

namespace Cnct.Core.Tests
{
    public class CnctRunnerTests
    {
        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAction_WhenMachineHasNoTags()
        {
            var action = new TestActionSpec();
            var (runner, actionRunner) = MakeRunner([], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAction_WhenMachineHasTags()
        {
            var action = new TestActionSpec();
            var (runner, actionRunner) = MakeRunner(["personal"], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_WhenMachineTagMatches()
        {
            var action = new TestActionSpec { Tags = ["personal"] };
            var (runner, actionRunner) = MakeRunner(["personal"], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsTaggedAction_WhenNoMachineTags()
        {
            var action = new TestActionSpec { Tags = ["personal"] };
            var (runner, actionRunner) = MakeRunner([], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ICnctActionSpec>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsTaggedAction_WhenMachineTagsDoNotMatch()
        {
            var action = new TestActionSpec { Tags = ["personal"] };
            var (runner, actionRunner) = MakeRunner(["work"], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ICnctActionSpec>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_CaseInsensitiveMatch()
        {
            var action = new TestActionSpec { Tags = ["Personal"] };
            var (runner, actionRunner) = MakeRunner(["personal"], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_WhenAnyTagMatches()
        {
            var action = new TestActionSpec { Tags = ["personal", "home"] };
            var (runner, actionRunner) = MakeRunner(["work", "home"], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAndSkipsTagged_Mixed()
        {
            var untaggedAction = new TestActionSpec();
            var taggedAction = new TestActionSpec { Tags = ["personal"] };
            var (runner, actionRunner) = MakeRunner(
                [], untaggedAction, taggedAction);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(
                    untaggedAction, It.IsAny<ILogger>(), "/"),
                Times.Once);
            actionRunner.Verify(
                r => r.ExecuteAsync(
                    taggedAction, It.IsAny<ILogger>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_LogsStartAndFinish_WithDisplayText()
        {
            var logger = new Mock<ILogger>();
            var actionRunner = new Mock<IActionRunner>();
            var action = new TestActionSpec();
            var config = new CnctConfig { Actions = [action] };
            var runner = new CnctRunner(config, logger.Object, "/", [], actionRunner.Object);

            await runner.ExecuteAsync();

            logger.Verify(l => l.LogStart("test"), Times.Once);
            logger.Verify(
                l => l.LogFinish(It.Is<string>(s => s.StartsWith("test"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_UsesLabel_WhenLabelIsSet()
        {
            var logger = new Mock<ILogger>();
            var actionRunner = new Mock<IActionRunner>();
            var action = new TestActionSpec { Label = "my custom label" };
            var config = new CnctConfig { Actions = [action] };
            var runner = new CnctRunner(config, logger.Object, "/", [], actionRunner.Object);

            await runner.ExecuteAsync();

            logger.Verify(l => l.LogStart("test: my custom label"), Times.Once);
            logger.Verify(
                l => l.LogFinish(It.Is<string>(s => s.StartsWith("test: my custom label"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_UsesGetDisplayText_WhenLabelIsNotSet()
        {
            var logger = new Mock<ILogger>();
            var actionRunner = new Mock<IActionRunner>();
            var action = new TestActionSpec();
            var config = new CnctConfig { Actions = [action] };
            var runner = new CnctRunner(config, logger.Object, "/", [], actionRunner.Object);

            await runner.ExecuteAsync();

            logger.Verify(l => l.LogStart("test"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsAction_WhenOsDoesNotMatch()
        {
            var logger = new Mock<ILogger>();
            var actionRunner = new Mock<IActionRunner>();
            var nonCurrentPlatform = Platform.CurrentPlatform == PlatformType.Windows
                ? PlatformType.Linux
                : PlatformType.Windows;
            var action = new TestActionSpec
            {
                PlatformType = [nonCurrentPlatform],
            };
            var config = new CnctConfig { Actions = [action] };
            var runner = new CnctRunner(config, logger.Object, "/", [], actionRunner.Object);

            await runner.ExecuteAsync();

            actionRunner.Verify(
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
                PlatformType = [Platform.CurrentPlatform],
            };
            var (runner, actionRunner) = MakeRunner([], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsAction_WhenOsNotSpecified()
        {
            var action = new TestActionSpec();
            var (runner, actionRunner) = MakeRunner([], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
            Assert.Null(action.PlatformType);
        }

        private static (CnctRunner Runner, Mock<IActionRunner> ActionRunner) MakeRunner(
            IReadOnlyCollection<string> machineTags,
            params ICnctActionSpec[] actions)
        {
            var actionRunner = new Mock<IActionRunner>();
            var config = new CnctConfig { Actions = actions };
            var runner = new CnctRunner(
                config, Mock.Of<ILogger>(), "/", machineTags, actionRunner.Object);

            return (runner, actionRunner);
        }

        private class TestActionSpec : CnctActionSpecBase
        {
            public override string ActionType => "test";
        }
    }
}
