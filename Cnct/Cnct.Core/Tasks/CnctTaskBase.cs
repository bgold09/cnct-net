using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal abstract class CnctTaskBase : ICnctTask
    {
        protected ILogger Logger { get; }

        protected CnctTaskBase(ILogger logger)
        {
            this.Logger = logger;
        }

        public abstract Task ExecuteAsync();
    }
}
