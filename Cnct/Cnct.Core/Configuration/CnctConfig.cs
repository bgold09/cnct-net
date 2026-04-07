using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public class CnctConfig
    {
        [JsonIgnore]
        public ILogger Logger { get; set; }

        [JsonIgnore]
        public string ConfigRootDirectory { get; set; }

        [JsonIgnore]
        public IReadOnlyCollection<string> MachineTags { get; set; } = Array.Empty<string>();

        [JsonProperty(ItemConverterType = typeof(CnctActionConverter))]
        public ICnctActionSpec[] Actions { get; set; }

        public void Validate()
        {
            foreach (var action in this.Actions)
            {
                action.Validate();
            }
        }

        public async Task<bool> ExecuteAsync()
        {
            foreach (var action in this.Actions.Where(a => a != null))
            {
                if (action is CnctActionSpecBase taggedAction
                    && taggedAction.Tags.Any()
                    && !taggedAction.Tags.Any(t => this.MachineTags.Contains(t, StringComparer.OrdinalIgnoreCase)))
                {
                    this.Logger.LogVerbose($"Skipping action '{action.ActionType}': no matching machine tag.");
                    continue;
                }

                string displayText = action is CnctActionSpecBase specBase
                    && !string.IsNullOrEmpty(specBase.Label)
                        ? $"{specBase.ActionType}: {specBase.Label}"
                        : action.GetDisplayText();

                try
                {
                    var start = DateTimeOffset.Now;
                    this.Logger.LogStart(displayText);

                    await action.ExecuteAsync(new IndentedLogger(this.Logger), this.ConfigRootDirectory);

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
