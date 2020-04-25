﻿using System;
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

        [JsonRequired]
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
