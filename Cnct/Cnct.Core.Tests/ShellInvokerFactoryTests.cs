using System;
using Cnct.Core.Tasks.Shell;
using Xunit;

namespace Cnct.Core.Tests
{
    public class ShellInvokerFactoryTests
    {
        private readonly ShellInvokerFactory factory = new ShellInvokerFactory();

        [Fact]
        public void Create_PowerShell_ReturnsPowerShellInvoker()
        {
            IShellInvoker invoker = this.factory.Create(
                Configuration.ShellTaskSpecification.ShellType.PowerShell,
                null);

            Assert.IsType<PowerShellInvoker>(invoker);
        }

        [Fact]
        public void Create_Sh_ReturnsShInvoker()
        {
            IShellInvoker invoker = this.factory.Create(
                Configuration.ShellTaskSpecification.ShellType.Sh,
                null);

            Assert.IsType<ShInvoker>(invoker);
        }

        [Fact]
        public void Create_Unknown_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this.factory.Create(
                    Configuration.ShellTaskSpecification.ShellType.Unknown,
                    null));
        }
    }
}
