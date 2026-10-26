using System.Diagnostics;
using Cnct.Core.Tasks.Shell;

namespace Cnct.Core.Tests
{
    public class PowerShellInvokerTests
    {
        [Fact]
        public async Task ExecuteAsync_ThrowsWhenOptionsIsNull()
        {
            var invoker = new PowerShellInvoker();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => invoker.ExecuteAsync(null));
        }

        [Fact]
        public async Task ExecuteAsync_CallsProcessRunnerWithPwsh()
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
                "./bootstrap.ps1", silent: false);

            var invoker = new PowerShellInvoker(
                logger.Object, runner.Object);
            await invoker.ExecuteAsync(options);

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    options,
                    logger.Object),
                Times.Once);

            Assert.Equal("pwsh", captured.FileName);
            Assert.Contains(
                "-NoProfile", captured.ArgumentList);
            Assert.Contains(
                "-NoLogo", captured.ArgumentList);
            Assert.Contains("-Command", captured.ArgumentList);
            Assert.Equal(
                "$ErrorActionPreference = 'Stop'; ./bootstrap.ps1",
                captured.ArgumentList.Last());
            Assert.False(captured.UseShellExecute);
            Assert.True(captured.RedirectStandardOutput);
            Assert.True(captured.RedirectStandardError);
            if (OperatingSystem.IsWindows())
            {
                Assert.Contains("-ExecutionPolicy", captured.ArgumentList);
                Assert.Contains("Unrestricted", captured.ArgumentList);
            }
        }

        [Fact]
        public async Task ExecuteAsync_SilentOptions_StillCallsRunner()
        {
            var logger = new Mock<ILogger>();
            var runner = new Mock<IProcessRunner>();
            runner.Setup(r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    It.IsAny<ShellExecutionOptions>(),
                    It.IsAny<ILogger>()))
                .Returns(Task.CompletedTask);

            var options = new ShellExecutionOptions(
                "./script.ps1", silent: true);

            var invoker = new PowerShellInvoker(
                logger.Object, runner.Object);
            await invoker.ExecuteAsync(options);

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    options,
                    logger.Object),
                Times.Once);
        }
    }
}
