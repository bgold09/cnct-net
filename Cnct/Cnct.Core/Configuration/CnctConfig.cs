using Cnct.Core.Tasks;
using Cnct.Core.Validation;

namespace Cnct.Core.Configuration
{
    public class CnctConfig
    {
        [JsonIgnore]
        public ILogger Logger { get; set; }

        [JsonIgnore]
        public string ConfigRootDirectory { get; set; }

        [JsonIgnore]
        public IReadOnlyCollection<string> MachineTags { get; set; } = [];

        [JsonIgnore]
        public IActionRunner Runner { get; set; } = new ActionRunner();

        [JsonProperty(ItemConverterType = typeof(CnctActionConverter))]
        public ICnctActionSpec[] Actions { get; set; }

        public ConfigValidationResult Validate()
        {
            if (this.Actions == null || this.Actions.Length == 0)
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
            var context = new CnctContext(this.ConfigRootDirectory, this.MachineTags);
            foreach (var action in this.Actions.Where(a => a != null))
            {
                issues.AddRange(action.Validate(context));
            }

            return new ConfigValidationResult(issues);
        }

        public async Task<bool> ExecuteAsync()
        {
            foreach (var action in this.Actions.Where(a => a != null))
            {
                if (action.Tags.Any()
                    && !action.Tags.Any(t => this.MachineTags.Contains(t, StringComparer.OrdinalIgnoreCase)))
                {
                    this.Logger.LogVerbose($"Skipping action '{action.ActionType}': no matching machine tag.");
                    continue;
                }

                if (!action.ShouldExecuteOnCurrentPlatform())
                {
                    this.Logger.LogVerbose($"Skipping action '{action.ActionType}': not applicable to current OS.");
                    continue;
                }

                string displayText = action.GetDisplayText();

                try
                {
                    var start = DateTimeOffset.Now;
                    this.Logger.LogStart(displayText);

                    await this.Runner.ExecuteAsync(action, new IndentedLogger(this.Logger), this.ConfigRootDirectory);

                    var end = DateTimeOffset.Now;
                    var elapsed = end - start;
                    this.Logger.LogFinish($"{displayText} ({elapsed.TotalSeconds:F1}s)");
                }
                catch (Exception ex)
                {
                    this.Logger.LogError($"Task of type '{action.ActionType}' failed.", ex);
                    return false;
                }
            }

            return true;
        }
    }
}
