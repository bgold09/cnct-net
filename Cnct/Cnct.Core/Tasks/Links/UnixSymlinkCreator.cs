using System.Runtime.InteropServices;

namespace Cnct.Core.Tasks
{
    internal class UnixSymlinkCreator : ISymlinkCreator
    {
        public void CreateSymlink(string linkPath, string targetPath, LinkType linkType, ILogger logger)
        {
            if (NativeMethods.CreateLinuxSymlink(targetPath, linkPath) != 0)
            {
                var errno = Marshal.GetLastWin32Error();
                logger.LogError($"Failed to create link (errno: {errno}).");
            }
        }
    }
}
