using System.Collections.Generic;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

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
    }
}
