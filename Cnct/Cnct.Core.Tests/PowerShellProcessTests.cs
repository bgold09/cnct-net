using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
using Cnct.Core.Tasks.Shell;

namespace Cnct.Core.Tests
{
    public class PowerShellProcessTests
    {
        [Fact]
        public async Task ExecuteAsync_StreamsPowerShellAndNativeOutput()
        {
            var logger = new Mock<ILogger>();
            var invoker = new PowerShellInvoker(logger.Object);
            string nativeCommand = OperatingSystem.IsWindows()
                ? "& $env:ComSpec /d /c 'echo native-output'"
                : "/bin/sh -c 'echo native-output'";

            await invoker.ExecuteAsync(new ShellExecutionOptions(
                "Write-Output 'pipeline-output'; Write-Host 'host-output'; " + nativeCommand,
                silent: false));

            foreach (string output in new[] { "pipeline-output", "host-output", "native-output" })
            {
                logger.Verify(l => l.LogInformation(output), Times.Once);
            }
        }

        [Theory]
        [InlineData("throw 'script-failed'")]
        [InlineData("Write-Error 'script-failed'; Write-Output 'should-not-run'")]
        public async Task ExecuteAsync_ReportsPowerShellErrors(string command)
        {
            var logger = new Mock<ILogger>();
            var invoker = new PowerShellInvoker(logger.Object);

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => invoker.ExecuteAsync(new ShellExecutionOptions(command, silent: false)));

            Assert.Contains(command, error.Message);
            Assert.Contains("exit code 1", error.Message);
            Assert.Contains("script-failed", error.Message);
            logger.Verify(l => l.LogInformation("should-not-run"), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ReportsNativeFailure()
        {
            var logger = new Mock<ILogger>();
            var invoker = new PowerShellInvoker(logger.Object);
            string command = OperatingSystem.IsWindows()
                ? "& $env:ComSpec /d /c 'exit /b 9'"
                : "/bin/sh -c 'exit 9'";

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => invoker.ExecuteAsync(new ShellExecutionOptions(command, silent: false)));

            Assert.Contains(command, error.Message);
            Assert.Contains("exit code", error.Message);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task ExecuteAsync_ReportsChildExitAndCapturesStderr(bool silent, bool abrupt)
        {
            var logger = new Mock<ILogger>();
            var invoker = new PowerShellInvoker(logger.Object);
            string command = "Write-Output 'stdout'; [Console]::Error.WriteLine('child-failed'); "
                + (abrupt ? "[Environment]::Exit(23)" : "exit 23");

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => invoker.ExecuteAsync(new ShellExecutionOptions(command, silent)));

            Assert.Contains("exit code 23", error.Message);
            Assert.Contains("child-failed", error.Message);
            logger.Verify(l => l.LogInformation("stdout"), silent ? Times.Never() : Times.Once());
            logger.Verify(l => l.LogWarning("child-failed"), silent ? Times.Never() : Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_WaitsForScriptThenRunsRemainingActions()
        {
            var logger = new Mock<ILogger>();
            var config = new CnctConfig
            {
                Actions =
                [
                    new ShellTaskSpecification
                    {
                        Shell = ShellTaskSpecification.ShellType.PowerShell,
                        Command = "Start-Sleep -Milliseconds 300; Write-Output 'script-finished'; exit 0",
                    },
                    new ShellTaskSpecification
                    {
                        Shell = ShellTaskSpecification.ShellType.PowerShell,
                        Command = "Write-Output 'remaining-action'",
                    },
                ],
            };
            var runner = new CnctRunner(
                config, logger.Object, Directory.GetCurrentDirectory(), [], new ActionRunner());

            Assert.True(await runner.ExecuteAsync());

            logger.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Trim() == "script-finished")), Times.Once);
            logger.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Trim() == "remaining-action")), Times.Once);
            logger.Verify(l => l.LogFinish(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_LogsChildFailureAndDoesNotRunRemainingActions()
        {
            var logger = new Mock<ILogger>();
            var config = new CnctConfig
            {
                Actions =
                [
                    new ShellTaskSpecification
                    {
                        Shell = ShellTaskSpecification.ShellType.PowerShell,
                        Command = "exit 23",
                    },
                    new ShellTaskSpecification
                    {
                        Shell = ShellTaskSpecification.ShellType.PowerShell,
                        Command = "Write-Output 'should-not-run'",
                    },
                ],
            };
            var runner = new CnctRunner(
                config, logger.Object, Directory.GetCurrentDirectory(), [], new ActionRunner());

            Assert.False(await runner.ExecuteAsync());

            logger.Verify(
                l => l.LogError(
                    It.IsAny<string>(),
                    It.Is<Exception>(e => e.Message.Contains("exit code 23"))),
                Times.Once);
            logger.Verify(l => l.LogStart(It.IsAny<string>()), Times.Once);
            logger.Verify(l => l.LogFinish(It.IsAny<string>()), Times.Never);
        }
    }
}
