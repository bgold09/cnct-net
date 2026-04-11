using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cnct.Core.Tasks.Shell;
using Moq;
using Xunit;

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
        public async Task ExecuteAsync_OnNonWindows_CallsProcessRunnerWithPwsh()
        {
            if (OperatingSystem.IsWindows())
            {
                return;
            }

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
            Assert.Contains("-File", captured.ArgumentList);
            Assert.Contains(
                "./bootstrap.ps1", captured.ArgumentList);
        }

        [Fact]
        public async Task ExecuteAsync_SilentOptions_StillCallsRunner()
        {
            if (OperatingSystem.IsWindows())
            {
                return;
            }

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
