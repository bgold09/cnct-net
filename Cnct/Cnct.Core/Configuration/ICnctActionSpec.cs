using System.Collections.Generic;
using Cnct.Core.Validation;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [JsonConverter(typeof(CnctActionConverter))]
    public interface ICnctActionSpec
    {
        string ActionType { get; }

        IReadOnlyCollection<string> Tags { get; }

        string GetDisplayText();

        bool ShouldExecuteOnCurrentPlatform();

        IReadOnlyList<ValidationIssue> Validate(string configDirectoryRoot);
    }
}
