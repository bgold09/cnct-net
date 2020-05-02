using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cnct.Core.Configuration
{
    public class ActionCollection : IEnumerable<ICnctActionSpec>
    {
        private readonly IReadOnlyCollection<ICnctActionSpec> actions;

        public ActionCollection(IReadOnlyCollection<ICnctActionSpec> actions)
        {
            this.actions = actions ?? throw new ArgumentNullException(nameof(actions));
        }

        public IEnumerator<ICnctActionSpec> GetEnumerator()
        {
            return this.actions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.actions.GetEnumerator();
        }
    }

    public class CnctConfig
    {
        [JsonIgnore]
        public ILogger Logger { get; set; }

        [JsonIgnore]
        public string ConfigRootDirectory { get; set; }

        [JsonConverter(typeof(ActionCollectionConverter))]
        public ActionCollection Actions { get; set; }

        public void Validate()
        {
            foreach (var action in this.Actions)
            {
                action.Validate();
            }
        }

        public async Task<bool> ExecuteAsync()
        {
            foreach (var action in this.Actions)
            {
                try
                {
                    await action.ExecuteAsync(this.Logger, this.ConfigRootDirectory);
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
