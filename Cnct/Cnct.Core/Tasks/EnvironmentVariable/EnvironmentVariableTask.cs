using System;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks.EnvironmentVariable
{
    internal class EnvironmentVariableTask : CnctTaskBase
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public EnvironmentVariableTask(ILogger logger, string name, string value)
            : base(logger)
        {
            this.Name = name;
            this.Value = value;
        }

        public override Task ExecuteAsync()
        {
            Environment.SetEnvironmentVariable(this.Name, this.Value, EnvironmentVariableTarget.User);
            this.Logger.LogInformation($"Set environment variable '{this.Name}' to '{this.Value}'.");

            return Task.CompletedTask;
        }
    }
}
