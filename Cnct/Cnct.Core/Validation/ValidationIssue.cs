using System.Text.Json.Serialization;

namespace Cnct.Core.Validation
{
    public class ValidationIssue
    {
        public ValidationIssue(ValidationSeverity severity, string actionType, string label, string message)
        {
            this.Severity = severity;
            this.ActionType = actionType;
            this.Label = label;
            this.Message = message;
        }

        [JsonPropertyName("severity")]
        public ValidationSeverity Severity { get; }

        [JsonPropertyName("actionType")]
        public string ActionType { get; }

        [JsonPropertyName("label")]
        public string Label { get; }

        [JsonPropertyName("message")]
        public string Message { get; }
    }
}
