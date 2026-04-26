using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class CnctRunnerActionFilterTests
    {
        [Fact]
        public async Task ExecuteAsync_RunsOnlyMatchingAction_WhenActionFilterIsSet()
        {
            var targetAction = new TestActionSpec { ID = "target" };
            var otherAction = new TestActionSpec { ID = "other" };
            var (runner, actionRunner) = MakeRunner([], ["target"], targetAction, otherAction);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(targetAction, It.IsAny<ILogger>(), "/"),
                Times.Once);
            actionRunner.Verify(
                r => r.ExecuteAsync(otherAction, It.IsAny<ILogger>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_RunsMultipleMatchingActions_WhenMultipleIdsInFilter()
        {
            var actionA = new TestActionSpec { ID = "a" };
            var actionB = new TestActionSpec { ID = "b" };
            var actionC = new TestActionSpec { ID = "c" };
            var (runner, actionRunner) = MakeRunner([], ["a", "c"], actionA, actionB, actionC);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(actionA, It.IsAny<ILogger>(), "/"),
                Times.Once);
            actionRunner.Verify(
                r => r.ExecuteAsync(actionB, It.IsAny<ILogger>(), It.IsAny<string>()),
                Times.Never);
            actionRunner.Verify(
                r => r.ExecuteAsync(actionC, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_MatchesActionIdCaseInsensitively()
        {
            var action = new TestActionSpec { ID = "My-Action" };
            var (runner, actionRunner) = MakeRunner([], ["my-action"], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(action, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsUnnamedActions_WhenActionFilterIsSet()
        {
            var namedAction = new TestActionSpec { ID = "named" };
            var unnamedAction = new TestActionSpec();
            var (runner, actionRunner) = MakeRunner([], ["named"], namedAction, unnamedAction);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(namedAction, It.IsAny<ILogger>(), "/"),
                Times.Once);
            actionRunner.Verify(
                r => r.ExecuteAsync(unnamedAction, It.IsAny<ILogger>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_StillSkipsAction_WhenOsDoesNotMatch()
        {
            var nonCurrentPlatform = Platform.CurrentPlatform == PlatformType.Windows
                ? PlatformType.Linux
                : PlatformType.Windows;
            var action = new TestActionSpec
            {
                ID = "os-limited",
                PlatformType = [nonCurrentPlatform],
            };
            var (runner, actionRunner) = MakeRunner([], ["os-limited"], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ICnctActionSpec>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_StillSkipsAction_WhenTagsDoNotMatch()
        {
            var action = new TestActionSpec
            {
                ID = "tagged",
                Tags = ["personal"],
            };
            var (runner, actionRunner) = MakeRunner(["work"], ["tagged"], action);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ICnctActionSpec>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenRequestedIdNotFound()
        {
            var logger = new Mock<ILogger>();
            var actionRunner = new Mock<IActionRunner>();
            var action = new TestActionSpec { ID = "exists" };
            var config = new CnctConfig { Actions = [action] };
            var runner = new CnctRunner(
                config, logger.Object, "/", [], actionRunner.Object, ["does-not-exist"]);

            await runner.ExecuteAsync();

            logger.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("does-not-exist") && s.Contains("not found")),
                    null),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RunsAllActions_WhenActionFilterIsEmpty()
        {
            var actionA = new TestActionSpec { ID = "a" };
            var actionB = new TestActionSpec { ID = "b" };
            var (runner, actionRunner) = MakeRunner([], [], actionA, actionB);

            await runner.ExecuteAsync();

            actionRunner.Verify(
                r => r.ExecuteAsync(actionA, It.IsAny<ILogger>(), "/"),
                Times.Once);
            actionRunner.Verify(
                r => r.ExecuteAsync(actionB, It.IsAny<ILogger>(), "/"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotLogNotFound_WhenOsSkippedNamedAction()
        {
            var nonCurrentPlatform = Platform.CurrentPlatform == PlatformType.Windows
                ? PlatformType.Linux
                : PlatformType.Windows;
            var logger = new Mock<ILogger>();
            var actionRunner = new Mock<IActionRunner>();
            var action = new TestActionSpec
            {
                ID = "os-limited",
                PlatformType = [nonCurrentPlatform],
            };
            var config = new CnctConfig { Actions = [action] };
            var runner = new CnctRunner(
                config, logger.Object, "/", [], actionRunner.Object, ["os-limited"]);

            await runner.ExecuteAsync();

            logger.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("not found")),
                    null),
                Times.Never);
        }

        [Fact]
        public void IdProperty_IsDeserialized_FromJson()
        {
            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "link",
                ["id"] = "my-links",
                ["links"] = new Dictionary<string, string> { ["a"] = "b" },
            });

            var spec = JsonConvert.DeserializeObject<ICnctActionSpec>(json);

            Assert.Equal("my-links", spec.ID);
        }

        private static (CnctRunner Runner, Mock<IActionRunner> ActionRunner) MakeRunner(
            IReadOnlyCollection<string> machineTags,
            IReadOnlyCollection<string> actionFilter,
            params ICnctActionSpec[] actions)
        {
            var actionRunner = new Mock<IActionRunner>();
            var config = new CnctConfig { Actions = actions };
            var runner = new CnctRunner(
                config, Mock.Of<ILogger>(), "/", machineTags, actionRunner.Object, actionFilter);

            return (runner, actionRunner);
        }

        private class TestActionSpec : CnctActionSpecBase
        {
            public override string ActionType => "test";
        }
    }
}
