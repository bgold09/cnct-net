using System.Collections.Generic;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("environmentVariable")]
    public partial class EnvironmentVariableTaskSpecification : CnctActionSpecBase
    {
        [JsonRequired]
        public string Name { get; set; }

        [JsonRequired]
        public string Value { get; set; }

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();
            if (!string.IsNullOrEmpty(this.Name) && this.Name.Contains('='))
            {
                issues.Add(this.CreateValidationError(
                    $"Environment variable '{this.Name}' contains '=', which is not valid."));
            }

            return issues;
        }

        protected override string GetAdditionalDisplayText() => $"'{this.Name}={this.Value}'";
    }
}
