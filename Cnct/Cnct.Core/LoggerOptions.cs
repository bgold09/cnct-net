using System;
using System.Collections.Generic;
using System.Text;

namespace Cnct.Core
{
    public class LoggerOptions
    {
        public bool Quiet { get; }

        public bool Debug { get; }

        public LoggerOptions(bool quiet, bool debug)
        {
            this.Quiet = quiet;
            this.Debug = debug;
        }
    }
}
