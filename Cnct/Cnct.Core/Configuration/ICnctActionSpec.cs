using Cnct.Core.Validation;

namespace Cnct.Core.Configuration
{
    [JsonConverter(typeof(CnctActionConverter))]
    public interface ICnctActionSpec
    {
        string ID { get; }

        string ActionType { get; }

        IReadOnlyCollection<string> Tags { get; }

        string GetDisplayText();

        bool ShouldExecuteOnCurrentPlatform();

        IReadOnlyList<ValidationIssue> Validate(CnctContext context);
    }
}
