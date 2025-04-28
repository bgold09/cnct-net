using System.Collections.Generic;

namespace Cnct.Core.Configuration
{
    public interface IFileManagement
    {
        IDictionary<string, IEnumerable<string>> GetFileConfigurations(
            string configDirectoryRoot,
            IReadOnlyDictionary<string, object> fileConfigs);
    }
}
