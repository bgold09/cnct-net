using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Cnct.Core.Configuration
{
    //[JsonConverter(typeof(LinkCollectionConverter))]
    public class SymlinkCollection : ReadOnlyCollection<string>
    {
        public SymlinkCollection(IList<string> list)
            : base(list)
        {
        }
    }
}
