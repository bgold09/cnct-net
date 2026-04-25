using Cnct.Core.Configuration;
using Cnct.Core.Validation;

namespace Cnct.Core.Tests
{
    public class EnvironmentVariableSpecificationTests
    {
        [Fact]
        public void CanDeserializeEnvironmentVariableSpec()
        {
            const string expectedVarName = "someVariable";
            const string expectedValue = "someValue";
            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "environmentVariable",
                ["name"] = expectedVarName,
                ["value"] = expectedValue,
            });

            ICnctActionSpec spec = JsonConvert.DeserializeObject<ICnctActionSpec>(json);

            EnvironmentVariableTaskSpecification envVariableSpec =
                Assert.IsType<EnvironmentVariableTaskSpecification>(spec);
            Assert.Equal(expectedVarName, envVariableSpec.Name);
            Assert.Equal(expectedValue, envVariableSpec.Value);
        }

        [Fact]
        public void GetDisplayText_ReturnsNameEqualsValue()
        {
            var spec = new EnvironmentVariableTaskSpecification
            {
                Name = "MY_VAR",
                Value = "hello",
            };

            Assert.Equal("environmentVariable: 'MY_VAR=hello'", spec.GetDisplayText());
        }

        [Fact]
        public void Validate_ReturnsNoIssues_WhenNameIsValid()
        {
            var spec = new EnvironmentVariableTaskSpecification
            {
                Name = "MY_VAR",
                Value = "hello",
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.Empty(issues);
        }

        [Fact]
        public void Validate_ReturnsError_WhenNameContainsEquals()
        {
            var spec = new EnvironmentVariableTaskSpecification
            {
                Name = "MY=VAR",
                Value = "hello",
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate(new CnctContext("/config", []));

            Assert.Single(issues);
            Assert.Equal(ValidationSeverity.Error, issues[0].Severity);
            Assert.Contains("'='", issues[0].Message);
        }
    }
}
