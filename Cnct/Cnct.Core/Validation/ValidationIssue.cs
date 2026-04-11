using System.Text.Json.Serialization;

namespace Cnct.Core.Validation
{
    public record ValidationIssue(
        [property: JsonPropertyName("severity")] ValidationSeverity Severity,
        [property: JsonPropertyName("actionType")] string ActionType,
        [property: JsonPropertyName("label")] string Label,
        [property: JsonPropertyName("message")] string Message);
}
