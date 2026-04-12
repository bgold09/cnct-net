using Cnct.Core.Validation;

namespace Cnct.Core.Configuration
{
    [CnctActionType("shell")]
    public partial class ShellTaskSpecification : CnctActionSpecBase
    {
        [JsonRequired]
        public ShellType Shell { get; set; }

        [JsonRequired]
        public string Command { get; set; }

        public bool Silent { get; set; }

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
