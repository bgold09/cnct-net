using Cnct.Core.Configuration;
using Cnct.Core.Tasks;
using Cnct.Core.Validation;

namespace Cnct.Core
{
    public class CnctRunner
    {
        private readonly CnctConfig config;
        private readonly ILogger logger;
        private readonly string configRootDirectory;
        private readonly IReadOnlyCollection<string> machineTags;
        private readonly IActionRunner runner;

        public CnctRunner(
            CnctConfig config,
            ILogger logger,
            string configRootDirectory,
            IReadOnlyCollection<string> machineTags,
            IActionRunner runner)
        {
            this.config = config;
            this.logger = logger;
            this.configRootDirectory = configRootDirectory;
            this.machineTags = machineTags;
            this.runner = runner;
        }

        public ConfigValidationResult Validate()
        {
            if (this.config.Actions == null || this.config.Actions.Length == 0)
            {
                return new ConfigValidationResult(new[]
                {
                    new ValidationIssue(
                        ValidationSeverity.Error,
                        "config",
                        null,
                        "The configuration must contain at least one action."),
                });
            }

            var issues = new List<ValidationIssue>();
            var context = new CnctContext(this.configRootDirectory, this.machineTags);
            foreach (var action in this.config.Actions.Where(a => a != null))
            {
                issues.AddRange(action.Validate(context));
            }

            return new ConfigValidationResult(issues);
        }

        public async Task<bool> ExecuteAsync()
        {
            foreach (var action in this.config.Actions.Where(a => a != null))
            {
                if (action.Tags.Any()
                    && !action.Tags.Any(t => this.machineTags.Contains(t, StringComparer.OrdinalIgnoreCase)))
                {
                    this.logger.LogVerbose($"Skipping action '{action.ActionType}': no matching machine tag.");
                    continue;
                }

                if (!action.ShouldExecuteOnCurrentPlatform())
                {
                    this.logger.LogVerbose(
                        $"Skipping action '{action.ActionType}': not applicable to current OS.");
                    continue;
                }

                string displayText = action.GetDisplayText();

                try
                {
                    var start = DateTimeOffset.Now;
                    this.logger.LogStart(displayText);

                    await this.runner.ExecuteAsync(
                        action, new IndentedLogger(this.logger), this.configRootDirectory);

                    var end = DateTimeOffset.Now;
                    var elapsed = end - start;
                    this.logger.LogFinish($"{displayText} ({elapsed.TotalSeconds:F1}s)");
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Task of type '{action.ActionType}' failed.", ex);
                    return false;
                }
            }

            return true;
        }
    }
}
