using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Tasks.Shell;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public class ShellTaskSpecification : ICnctActionSpec
    {
        private readonly HashSet<PlatformType> platformTypes = new HashSet<PlatformType>();

        public string ActionType { get; } = "shell";

        [JsonRequired]
        public ShellType Shell { get; set; }

        [JsonRequired]
        public string Command { get; set; }

        [JsonProperty("os")]
        [JsonRequired]
        [JsonConverter(typeof(LinkCollectionConverter))]
        public string[] PlatformType { get; set; }

        public bool Silent { get; set; }

        public async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            if (!this.platformTypes.Contains(Platform.CurrentPlatform))
            {
                return;
            }

            IShellInvoker shellInvoker = this.Shell switch
            {
                ShellType.PowerShell => new PowerShellInvoker(),
                _ => throw new IndexOutOfRangeException(),
            };

            await shellInvoker.ExecuteAsync(this);
        }

        public void Validate()
        {
            if (this.Shell == ShellType.Unknown)
            {
                throw new ArgumentOutOfRangeException($"Shell type not recognized.");
            }

            if (string.IsNullOrWhiteSpace(this.Command))
            {
                throw new ArgumentException("A command must be specified.");
            }

            if (!this.PlatformType.Any())
            {
                throw new ArgumentException("At least one OS must be specified.");
            }

            foreach (string item in this.PlatformType)
            {
                if (!Enum.TryParse(item, true, out PlatformType platformType))
                {
                    throw new ArgumentOutOfRangeException($"OS type '{item}' was not recognized.");
                }

                this.platformTypes.Add(platformType);
            }
        }

        public enum ShellType
        {
            Unknown = 0,
            PowerShell,
        }
    }
}
