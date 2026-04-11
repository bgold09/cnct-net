using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cnct.Core.Tasks.EnvironmentVariable;
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

        public override async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var envVariableTask = new EnvironmentVariableTask(logger, this.Name, this.Value);
            await envVariableTask.ExecuteAsync();
        }

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();

            if (!string.IsNullOrEmpty(this.Name) && this.Name.Contains('='))
            {
                issues.Add(this.CreateValidationIssue(
                    ValidationSeverity.Error,
                    $"Environment variable '{this.Name}' contains '=', which is not valid."));
            }

            return issues;
        }

        protected override string GetAdditionalDisplayText() => $"'{this.Name}={this.Value}'";
    }
}
