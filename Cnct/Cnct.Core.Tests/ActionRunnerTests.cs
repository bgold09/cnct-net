using Cnct.Core.Configuration;
using Cnct.Core.Tasks;

namespace Cnct.Core.Tests
{
    public class ActionRunnerTests
    {
        [Fact]
        public async Task ExecuteAsync_ThrowsForUnsupportedSpecType()
        {
            var runner = new ActionRunner();
            var spec = new UnsupportedSpec();

            await Assert.ThrowsAsync<NotSupportedException>(
                () => runner.ExecuteAsync(
                    spec, Mock.Of<ILogger>(), "/config"));
        }

        [Fact]
        public async Task LinkExpandDispatch_ResolvesRelativeSource()
        {
            string configRoot = Path.GetTempPath();
            const string relativeSource = "skills";
            string expectedSource =
                Path.Combine(configRoot, relativeSource);

            var logger = new Mock<ILogger>();
            var runner = new ActionRunner();
            var spec = new LinkExpandTaskSpecification
            {
                Source = relativeSource,
                Target = "~/some/target",
            };

            await runner.ExecuteAsync(
                spec, logger.Object, configRoot);

            logger.Verify(
                l => l.LogWarning(
                    It.Is<string>(
                        s => s.Contains(expectedSource))),
                Times.Once);
        }

        [Fact]
        public async Task CloneGitRepositoryDispatch_ResolvesRelativeDest()
        {
            string configRoot = Path.GetTempPath();
            const string url = "https://example.com/org/repo-a";
            string relativeDest = Path.Combine("dev", "repo-a");
            string expectedDest =
                Path.Combine(configRoot, relativeDest);

            var logger = new Mock<ILogger>();
            var runner = new ActionRunner();
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    [url] = relativeDest,
                },
            };

            // ExecuteAsync will fail (no real git), but only
            // after path resolution has occurred. We verify via
            // the logged clone message containing the resolved
            // absolute path.
            try
            {
                await runner.ExecuteAsync(
                    spec, logger.Object, configRoot);
            }
            catch
            {
                // Expected: git process not available in tests
            }

            logger.Verify(
                l => l.LogInformation(
                    It.Is<string>(
                        s => s.Contains(expectedDest))),
                Times.Once);
        }

        private class UnsupportedSpec : CnctActionSpecBase
        {
            public override string ActionType => "unsupported";
        }
    }
}
