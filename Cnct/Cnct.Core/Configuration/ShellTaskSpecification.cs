using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Tasks.Shell;
using Cnct.Core.Validation;
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

            await shellInvoker.ExecuteAsync(this);
        }

        public override IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            var issues = new List<ValidationIssue>();

            if (this.Shell == ShellType.Unknown)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    this.ActionType,
                    this.Label,
                    $"Shell type '{this.Shell}' not recognized."));
            }

            if (string.IsNullOrWhiteSpace(this.Command))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    this.ActionType,
                    this.Label,
                    "A command must be specified."));
            }

            string executable = this.Shell switch
            {
                ShellType.PowerShell => "pwsh",
                ShellType.Sh => "sh",
                _ => null,
            };

            if (executable != null && !IsOnPath(executable))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    this.ActionType,
                    this.Label,
                    $"Shell executable '{executable}' was not found on PATH."));
            }

            return issues;
        }

        protected override string GetAdditionalDisplayText()
        {
            string shellName = this.Shell.ToString();
            string camelShell = char.ToLowerInvariant(shellName[0]) + shellName.Substring(1);
            return $"'{camelShell} {this.Command}'";
        }

        private static bool IsOnPath(string executable)
        {
            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] pathDirs = pathEnv.Split(Path.PathSeparator);
            bool isWindows = Platform.CurrentPlatform == global::Cnct.Core.Configuration.PlatformType.Windows;
            string[] extensions = isWindows
                ? new[] { ".exe", ".cmd", ".bat" }
                : new[] { string.Empty };

            return pathDirs.Any(dir => extensions.Any(ext =>
                File.Exists(Path.Combine(dir, executable + ext))));
        }

        public enum ShellType
        {
            Unknown = 0,
            PowerShell,
            Sh,
        }
    }
}
