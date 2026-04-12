using System.Diagnostics;
using Cnct.Core.Tasks.Shell;

namespace Cnct.Core.Tests
{
    public class ShInvokerTests
    {
        [Fact]
        public async Task ExecuteAsync_ThrowsWhenOptionsIsNull()
        {
            var logger = new Mock<ILogger>();
            var invoker = new ShInvoker(logger.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => invoker.ExecuteAsync(null));
        }

        [Fact]
        public async Task ExecuteAsync_CallsProcessRunnerWithShArgs()
        {
            var logger = new Mock<ILogger>();
            var runner = new Mock<IProcessRunner>();
            ProcessStartInfo captured = null;

            runner.Setup(r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    It.IsAny<ShellExecutionOptions>(),
                    It.IsAny<ILogger>()))
                .Callback<ProcessStartInfo,
                    ShellExecutionOptions, ILogger>(
                    (si, _, __) => captured = si)
                .Returns(Task.CompletedTask);

            var options = new ShellExecutionOptions(
                "./bootstrap.sh", silent: false);

            var invoker = new ShInvoker(
                logger.Object, runner.Object);
            await invoker.ExecuteAsync(options);

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    options,
                    logger.Object),
                Times.Once);

            Assert.Equal("/bin/sh", captured.FileName);
            Assert.Contains("-c", captured.ArgumentList);
            Assert.Contains(
                "./bootstrap.sh", captured.ArgumentList);
        }
    }
}
