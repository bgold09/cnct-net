using System.Runtime.InteropServices;

namespace Cnct.Core.Tasks
{
    internal class WindowsSymlinkCreator : ISymlinkCreator
    {
        public void CreateSymlink(string linkPath, string targetPath, LinkType linkType, ILogger logger)
        {
            if (!NativeMethods.CreateSymbolicLink(linkPath, targetPath, linkType))
            {
                int hr = Marshal.GetHRForLastWin32Error();
                logger.LogError($"Failed to create link.", Marshal.GetExceptionForHR(hr));
            }
        }
    }
}
