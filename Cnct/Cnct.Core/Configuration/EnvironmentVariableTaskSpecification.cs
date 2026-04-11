using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Tasks.EnvironmentVariable;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("environmentVariable")]
    public partial class EnvironmentVariableTaskSpecification : CnctActionSpecBase
    {
        private readonly IEnvironmentVariableWriter writer;

        public EnvironmentVariableTaskSpecification(
            IEnvironmentVariableWriter writer)
        {
            this.writer = writer;
        }

        public EnvironmentVariableTaskSpecification()
            : this(new EnvironmentVariableWriter())
        {
        }

        [JsonRequired]
        public string Name { get; set; }

        [JsonRequired]
        public string Value { get; set; }

        public override async Task ExecuteAsync(
            ILogger logger,
            string configDirectoryRoot)
        {
            var envVariableTask = new EnvironmentVariableTask(
                logger,
                this.writer,
                this.Name,
                this.Value);
            await envVariableTask.ExecuteAsync();
        }

        public override IReadOnlyList<ValidationIssue> Validate(
            string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();
            if (!string.IsNullOrEmpty(this.Name)
                && this.Name.Contains('='))
            {
                issues.Add(this.CreateValidationError(
                    $"Environment variable '{this.Name}' "
                    + "contains '=', which is not valid."));
            }

            return issues;
        }

        protected override string GetAdditionalDisplayText() =>
            $"'{this.Name}={this.Value}'";
    }
}
