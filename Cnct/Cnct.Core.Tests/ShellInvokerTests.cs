using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Cnct.Core.Tasks.Shell;
using Moq;
using Xunit;

namespace Cnct.Core.Tests
{
    public class ShInvokerTests
    {
        [Fact]
        public async Task ExecuteAsync_ThrowsWhenSpecIsNull()
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
                    It.IsAny<ShellTaskSpecification>(),
                    It.IsAny<ILogger>()))
                .Callback<ProcessStartInfo,
                    ShellTaskSpecification, ILogger>(
                    (si, _, __) => captured = si)
                .Returns(Task.CompletedTask);

            var spec = new ShellTaskSpecification
            {
                Shell = ShellTaskSpecification.ShellType.Sh,
                Command = "./bootstrap.sh",
            };

            var invoker = new ShInvoker(
                logger.Object, runner.Object);
            await invoker.ExecuteAsync(spec);

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    spec,
                    logger.Object),
                Times.Once);

            Assert.Equal("/bin/sh", captured.FileName);
            Assert.Contains("-c", captured.ArgumentList);
            Assert.Contains(
                "./bootstrap.sh", captured.ArgumentList);
        }
    }

    public class PowerShellInvokerTests
    {
        [Fact]
        public async Task ExecuteAsync_ThrowsWhenSpecIsNull()
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
                    It.IsAny<ShellTaskSpecification>(),
                    It.IsAny<ILogger>()))
                .Callback<ProcessStartInfo,
                    ShellTaskSpecification, ILogger>(
                    (si, _, __) => captured = si)
                .Returns(Task.CompletedTask);

            var spec = new ShellTaskSpecification
            {
                Shell =
                    ShellTaskSpecification.ShellType.PowerShell,
                Command = "./bootstrap.ps1",
            };

            var invoker = new PowerShellInvoker(
                logger.Object, runner.Object);
            await invoker.ExecuteAsync(spec);

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    spec,
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
        public async Task ExecuteAsync_SilentSpec_StillCallsRunner()
        {
            if (OperatingSystem.IsWindows())
            {
                return;
            }

            var logger = new Mock<ILogger>();
            var runner = new Mock<IProcessRunner>();
            runner.Setup(r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    It.IsAny<ShellTaskSpecification>(),
                    It.IsAny<ILogger>()))
                .Returns(Task.CompletedTask);

            var spec = new ShellTaskSpecification
            {
                Shell =
                    ShellTaskSpecification.ShellType.PowerShell,
                Command = "./script.ps1",
                Silent = true,
            };

            var invoker = new PowerShellInvoker(
                logger.Object, runner.Object);
            await invoker.ExecuteAsync(spec);

            runner.Verify(
                r => r.ExecuteAsync(
                    It.IsAny<ProcessStartInfo>(),
                    spec,
                    logger.Object),
                Times.Once);
        }
    }
}
