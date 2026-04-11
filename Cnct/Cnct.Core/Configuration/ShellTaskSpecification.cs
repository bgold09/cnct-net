using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Tasks.Shell;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("shell")]
    public partial class ShellTaskSpecification : CnctActionSpecBase
    {
        private readonly IShellInvokerFactory shellInvokerFactory;

        public ShellTaskSpecification()
            : this(new ShellInvokerFactory())
        {
        }

        public ShellTaskSpecification(IShellInvokerFactory shellInvokerFactory)
        {
            this.shellInvokerFactory = shellInvokerFactory;
        }

        [JsonRequired]
        public ShellType Shell { get; set; }

        [JsonRequired]
        public string Command { get; set; }

        public bool Silent { get; set; }

        public override async Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
        {
            IShellInvoker shellInvoker = this.Shell switch
            {
                ShellType.PowerShell => new PowerShellInvoker(logger),
                ShellType.Sh => new ShInvoker(logger),
                _ => throw new ArgumentOutOfRangeException(
                    message: $"Shell type {this.Shell} is not supported.",
                    innerException: null),
            };

            var options = new ShellExecutionOptions(
                this.Command, this.Silent);
            await shellInvoker.ExecuteAsync(options);
        }

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();
            if (this.Shell == ShellType.Unknown)
            {
                issues.Add(this.CreateValidationError($"Shell type '{this.Shell}' not recognized."));
            }

            if (string.IsNullOrWhiteSpace(this.Command))
            {
                issues.Add(this.CreateValidationError("A command must be specified."));
            }

            return issues;
        }

        protected override string GetAdditionalDisplayText()
        {
            string shellName = this.Shell.ToString();
            string camelShell = char.ToLowerInvariant(shellName[0]) + shellName.Substring(1);
            return $"'{camelShell} {this.Command}'";
        }

        public enum ShellType
        {
            Unknown = 0,
            PowerShell,
            Sh,
        }
    }
}
