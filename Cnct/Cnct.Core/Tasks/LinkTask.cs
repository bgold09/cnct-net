using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal class LinkTask : CnctTaskBase
    {
        private readonly IDictionary<string, IEnumerable<string>> links;

        public LinkTask(ILogger logger, IDictionary<string, IEnumerable<string>> links)
            : base(logger)
        {
            this.links = links;
        }

        public override Task ExecuteAsync()
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
            if (linkType == LinkType.File)
            {
                File.Delete(linkPath);
            }
            else if (linkType == LinkType.Directory)
            {
                Directory.Delete(linkPath, recursive: true);
            }

            if (!NativeMethods.CreateSymbolicLink(linkPath, targetPath, linkType))
            {
                int hr = Marshal.GetHRForLastWin32Error();
            }
        }
    }
}
