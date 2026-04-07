using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cnct.Core.Tasks.EnvironmentVariable;
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

        protected override string GetAdditionalDisplayText() =>
            $"'{this.Name}={this.Value}'";

        public override async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var envVariableTask = new EnvironmentVariableTask(logger, this.Name, this.Value);
            await envVariableTask.ExecuteAsync();
        }

        public override void Validate()
        {
        }
    }
}
