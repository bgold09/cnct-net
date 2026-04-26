using System.Collections.Generic;
using System.Linq;
using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
using Cnct.Core.Validation;
using Moq;
using Xunit;

namespace Cnct.Core.Tests
{
    public class CnctRunnerDuplicateIdValidationTests
    {
        [Fact]
        public void Validate_ReturnsError_WhenDuplicateIdsExist()
        {
            var runner = MakeRunner(new TestActionSpec { ID = "my-action" }, new TestActionSpec { ID = "my-action" });

            ConfigValidationResult result = runner.Validate();

            Assert.False(result.IsValid);
            Assert.Contains(
                result.Issues,
                i => i.Severity == ValidationSeverity.Error
                    && i.Message.Contains("Duplicate action id 'my-action'"));
        }

        [Fact]
        public void Validate_ReturnsError_WhenDuplicateIdsCaseInsensitive()
        {
            var runner = MakeRunner(
                new TestActionSpec { ID = "My-Action" }, new TestActionSpec { ID = "my-action" });

            ConfigValidationResult result = runner.Validate();

            Assert.False(result.IsValid);
            Assert.Single(
                result.Issues,
                i => i.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void Validate_Succeeds_WhenIdsAreUnique()
        {
            var runner = MakeRunner(
                new TestActionSpec { ID = "action-a" }, new TestActionSpec { ID = "action-b" });

            ConfigValidationResult result = runner.Validate();

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_Succeeds_WhenMultipleActionsHaveNoId()
        {
            var runner = MakeRunner(new TestActionSpec(), new TestActionSpec());

            ConfigValidationResult result = runner.Validate();

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_Succeeds_WhenMixOfNamedAndUnnamedActions()
        {
            var runner = MakeRunner(
                new TestActionSpec { ID = "my-action" }, new TestActionSpec(), new TestActionSpec());

            ConfigValidationResult result = runner.Validate();

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ReportsMultipleDuplicateGroups()
        {
            var runner = MakeRunner(
                new TestActionSpec { ID = "alpha" },
                new TestActionSpec { ID = "alpha" },
                new TestActionSpec { ID = "beta" },
                new TestActionSpec { ID = "beta" });

            ConfigValidationResult result = runner.Validate();

            Assert.False(result.IsValid);
            Assert.Equal(
                2,
                result.Issues.Count(i => i.Severity == ValidationSeverity.Error
                    && i.Message.Contains("Duplicate action id")));
        }

        private static CnctRunner MakeRunner(params ICnctActionSpec[] actions)
        {
            var config = new CnctConfig { Actions = actions };
            return new CnctRunner(
                config, Mock.Of<ILogger>(), "/", [], Mock.Of<IActionRunner>());
        }

        private class TestActionSpec : CnctActionSpecBase
        {
            public override string ActionType => "test";
        }
    }
}
