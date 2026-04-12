using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Cnct.Core.Validation
{
    public class ConfigValidationResult
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        public ConfigValidationResult(IReadOnlyList<ValidationIssue> issues)
        {
            this.Issues = issues;
        }

        [JsonPropertyName("valid")]
        public bool IsValid => this.Issues.All(i => i.Severity != ValidationSeverity.Error);

        [JsonPropertyName("issues")]
        public IReadOnlyList<ValidationIssue> Issues { get; }

        public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);
    }
}
