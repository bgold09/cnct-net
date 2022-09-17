using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Tasks.Shell;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("shell")]
    public partial class ShellTaskSpecification : ICnctActionSpec
    {
        [JsonRequired]
        public ShellType Shell { get; set; }

        [JsonRequired]
        public string Command { get; set; }

        [JsonProperty("os")]
        [JsonRequired]
        [JsonConverter(typeof(EnumCollectionConverter<PlatformType>))]
        public IReadOnlyCollection<PlatformType> PlatformType { get; set; }

        public bool Silent { get; set; }

        public async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            if (!this.PlatformType.Contains(Platform.CurrentPlatform))
            {
                return;
            }

            IShellInvoker shellInvoker = this.Shell switch
            {
                ShellType.PowerShell => new PowerShellInvoker(),
                _ => throw new ArgumentOutOfRangeException(
                    message: $"Shell type {this.Shell} is not supported.",
                    innerException: null),
            };

            await shellInvoker.ExecuteAsync(this);
        }

        public void Validate()
        {
            if (this.Shell == ShellType.Unknown)
            {
                throw new ArgumentException($"Shell type '{this.Shell}' not recognized.");
            }

            if (string.IsNullOrWhiteSpace(this.Command))
            {
                throw new ArgumentException("A command must be specified.");
            }

            if (!this.PlatformType.Any())
            {
                throw new ArgumentException("At least one valid OS must be specified.");
            }
        }

        public enum ShellType
        {
            Unknown = 0,
            PowerShell,
        }
    }
}
