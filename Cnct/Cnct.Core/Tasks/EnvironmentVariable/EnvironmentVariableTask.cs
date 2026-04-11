using System;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks.EnvironmentVariable
{
    internal class EnvironmentVariableTask : CnctTaskBase
    {
        private readonly IEnvironmentVariableWriter writer;

        public string Name { get; set; }

        public string Value { get; set; }

        public EnvironmentVariableTask(
            ILogger logger,
            IEnvironmentVariableWriter writer,
            string name,
            string value)
            : base(logger)
        {
            this.writer = writer;
            this.Name = name;
            this.Value = value;
        }

        public override Task ExecuteAsync()
        {
            this.writer.SetVariable(this.Name, this.Value);
            this.Logger.LogInformation($"Set environment variable '{this.Name}' to '{this.Value}'.");

            return Task.CompletedTask;
        }
    }
}
