using System;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
using Moq;
using Xunit;

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

        private class UnsupportedSpec : CnctActionSpecBase
        {
            public override string ActionType => "unsupported";
        }
    }
}
