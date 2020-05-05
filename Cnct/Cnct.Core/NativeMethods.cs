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

        [DllImport("libc.so.6", EntryPoint = "symlink", CharSet = CharSet.Unicode)]
        public static extern int CreateLinuxSymlink(string targetPath, string linkPath);
    }
}
