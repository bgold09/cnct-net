using System;
using System.Diagnostics;
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
}
