using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks.EnvironmentVariable
{
    internal partial class EnvironmentVariableTask
    {
        private readonly IEnvironmentVariableWriter writer;

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

        public static partial EnvironmentVariableTask FromTaskSpecification(
            EnvironmentVariableTaskSpecification spec,
            ILogger logger,
            string configDirectoryRoot)
        {
            return new EnvironmentVariableTask(logger, new EnvironmentVariableWriter(), spec.Name, spec.Value);
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public override Task ExecuteAsync()
        {
            this.writer.SetVariable(this.Name, this.Value);
            this.Logger.LogInformation($"Set environment variable '{this.Name}' to '{this.Value}'.");

            return Task.CompletedTask;
        }
    }
}
