using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

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
                    this.Logger.LogInformation($"  [LINK] {target} -> {link}");
                    if (File.Exists(target))
                    {
                        this.CreateLink(link, target, LinkType.File);
                    }
                    else if (Directory.Exists(target))
                    {
                        this.CreateLink(link, target, LinkType.Directory);
                    }
                    else
                    {
                        this.Logger.LogWarning($"Target '{target}' does not exist.");
                    }
                }
            }

            return Task.FromResult(0);
        }

        private void CreateLink(string linkPath, string targetPath, LinkType linkType)
        {
            if (File.Exists(linkPath))
            {
                File.Delete(linkPath);
            }
            else if (Directory.Exists(linkPath))
            {
                Directory.Delete(linkPath);
            }

            switch (Platform.CurrentPlatform)
            {
                case PlatformType.Windows:
                    if (!NativeMethods.CreateSymbolicLink(linkPath, targetPath, linkType))
                    {
                        int hr = Marshal.GetHRForLastWin32Error();
                        this.Logger.LogError($"Failed to create link.", Marshal.GetExceptionForHR(hr));
                    }

                    break;

                case PlatformType.Linux:
                    if (NativeMethods.CreateLinuxSymlink(targetPath, linkPath) != 0)
                    {
                        this.Logger.LogError($"Failed to create link.");
                    }

                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
