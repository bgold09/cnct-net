using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Tasks.Shell;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("shell")]
    public partial class ShellTaskSpecification : CnctActionSpecBase
    {
        [JsonRequired]
        public ShellType Shell { get; set; }

        [JsonRequired]
        public string Command { get; set; }

        [JsonProperty("os")]
        [JsonConverter(typeof(EnumCollectionConverter<PlatformType>))]
        public IReadOnlyCollection<PlatformType> PlatformType { get; set; }

        public bool Silent { get; set; }

        protected override string GetAdditionalDisplayText() =>
            $"'{this.Shell} {this.Command}'";

        public override async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            if (this.PlatformType?.Count > 0
                && !this.PlatformType.Contains(Platform.CurrentPlatform))
            {
                return;
            }

            IShellInvoker shellInvoker = this.Shell switch
            {
                ShellType.PowerShell => new PowerShellInvoker(logger),
                ShellType.Sh => new ShInvoker(logger),
                _ => throw new ArgumentOutOfRangeException(
                    message: $"Shell type {this.Shell} is not supported.",
                    innerException: null),
            };

            await shellInvoker.ExecuteAsync(this);
        }

        public override void Validate()
        {
            if (this.Shell == ShellType.Unknown)
            {
                throw new ArgumentException(
                    $"Shell type '{this.Shell}' not recognized.");
            }

            if (string.IsNullOrWhiteSpace(this.Command))
            {
                throw new ArgumentException("A command must be specified.");
            }
        }

        public enum ShellType
        {
            Unknown = 0,
            PowerShell,
            Sh,
        }
    }
}
