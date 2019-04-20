using System.Runtime.InteropServices;
using Cnct.Core.Tasks;

namespace Cnct.Core
{
    internal static partial class NativeMethods
    {
        [DllImport("Kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode)]
        public static extern bool CreateSymbolicLink(
            string symlinkFileName,
            string targetFileName,
            LinkType flags);
    }
}
