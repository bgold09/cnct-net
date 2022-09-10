using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cnct.Core.Tasks.EnvironmentVariable;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public class EnvironmentVariableTaskSpecification : ICnctActionSpec
    {
        public string ActionType { get; } = "environmentVariable";

        [JsonRequired]
        public string Name { get; set; }

        [JsonRequired]
        public string Value { get; set; }

        public async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            var envVariableTask = new EnvironmentVariableTask(logger, this.Name, this.Value);
            await envVariableTask.ExecuteAsync();
        }

        public void Validate()
        {
        }
    }
}
