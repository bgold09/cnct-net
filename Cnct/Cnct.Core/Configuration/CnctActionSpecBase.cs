using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public abstract class CnctActionSpecBase : ICnctActionSpec
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("tags")]
        [JsonConverter(typeof(StringCollectionConverter))]
        public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();

        [JsonProperty("os")]
        [JsonConverter(typeof(EnumCollectionConverter<PlatformType>))]
        public IReadOnlyCollection<PlatformType> PlatformType { get; set; }

        public abstract string ActionType { get; }

        public string GetDisplayText()
        {
            if (!string.IsNullOrEmpty(this.Label))
            {
                return $"{this.ActionType}: {this.Label}";
            }

            string extra = this.GetAdditionalDisplayText();
            return string.IsNullOrEmpty(extra)
                ? this.ActionType
                : $"{this.ActionType}: {extra}";
        }

        public bool ShouldExecuteOnCurrentPlatform()
        {
            return this.PlatformType == null
                || this.PlatformType.Count == 0
                || this.PlatformType.Contains(Platform.CurrentPlatform);
        }

        public virtual IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot)
        {
            return Array.Empty<ValidationIssue>();
        }

        public abstract Task ExecuteAsync(ILogger logger, string configDirectoryRoot);

        protected ValidationIssue CreateValidationError(string message)
        {
            return this.CreateValidationIssue(ValidationSeverity.Error, message);
        }

        protected ValidationIssue CreateValidationWarning(string message)
        {
            return this.CreateValidationIssue(ValidationSeverity.Warning, message);
        }

        protected virtual string GetAdditionalDisplayText() => null;

        private ValidationIssue CreateValidationIssue(ValidationSeverity severity, string message)
        {
            return new ValidationIssue(severity, this.ActionType, this.Label, message);
        }
    }
}
