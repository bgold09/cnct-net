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
        private readonly IReadOnlyCollection<string> actionFilter;

        public CnctRunner(
            CnctConfig config,
            ILogger logger,
            string configRootDirectory,
            IReadOnlyCollection<string> machineTags,
            IActionRunner runner,
            IReadOnlyCollection<string> actionFilter = null)
        {
            this.config = config;
            this.logger = logger;
            this.configRootDirectory = configRootDirectory;
            this.machineTags = machineTags;
            this.runner = runner;
            this.actionFilter = actionFilter ?? [];
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
            var seenIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var action in this.config.Actions.Where(a => a != null))
            {
                issues.AddRange(action.Validate(context));

                if (!string.IsNullOrEmpty(action.ID))
                {
                    if (!seenIds.TryAdd(action.ID, 1))
                    {
                        seenIds[action.ID]++;
                    }
                }
            }

            foreach (var entry in seenIds.Where(e => e.Value > 1))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "config",
                    null,
                    $"Duplicate action id '{entry.Key}' found on {entry.Value} actions."));
            }

            return new ConfigValidationResult(issues);
        }

        public async Task<bool> ExecuteAsync()
        {
            bool hasActionFilter = this.actionFilter.Count > 0;
            var matchedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var action in this.config.Actions.Where(a => a != null))
            {
                string skipReason = this.GetSkipReason(action, hasActionFilter, matchedIds);
                if (skipReason != null)
                {
                    this.logger.LogVerbose($"Skipping action '{action.GetDisplayText()}': {skipReason}.");
                    continue;
                }

                try
                {
                    string displayText = action.GetDisplayText();
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

            if (hasActionFilter)
            {
                foreach (var id in this.actionFilter)
                {
                    if (!matchedIds.Contains(id))
                    {
                        this.logger.LogError($"Action with id '{id}' was not found in the configuration.");
                    }
                }
            }

            return true;
        }

        private string GetSkipReason(ICnctActionSpec action, bool hasActionFilter, HashSet<string> matchedIds)
        {
            if (hasActionFilter)
            {
                if (string.IsNullOrEmpty(action.ID)
                    || !this.actionFilter.Contains(action.ID, StringComparer.OrdinalIgnoreCase))
                {
                    return "not targeted by --action filter";
                }

                matchedIds.Add(action.ID);
            }

            if (action.Tags.Any()
                && !action.Tags.Any(t => this.machineTags.Contains(t, StringComparer.OrdinalIgnoreCase)))
            {
                return "no matching machine tag";
            }

            if (!action.ShouldExecuteOnCurrentPlatform())
            {
                return "not applicable to current OS";
            }

            return null;
        }
    }
}
