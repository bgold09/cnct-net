using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal class LinkTask : ICnctTask
    {
        private readonly IDictionary<string, IEnumerable<string>> links;

        public LinkTask(IDictionary<string, IEnumerable<string>> links)
        {
            this.links = links;
        }

        public Task ExecuteAsync()
        {
            foreach (var kvp in this.links)
            {
                string target = kvp.Key;
                IEnumerable<string> destinationLinks = kvp.Value;

                foreach (string link in destinationLinks)
                {
                    if (File.Exists(target))
                    {
                        CreateLink(link, target, LinkType.File);
                    }
                    else if (Directory.Exists(target))
                    {
                        CreateLink(link, target, LinkType.Directory);
                    }
                }
            }

            return Task.FromResult(0);
        }

        private static void CreateLink(string linkPath, string targetPath, LinkType linkType)
        {
            if (!NativeMethods.CreateSymbolicLink(linkPath, targetPath, linkType))
            {
                int hr = Marshal.GetHRForLastWin32Error();
            }
        }
    }
}
