using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Configuration;

namespace Cnct.Core.Tasks
{
    internal class LinkTask : CnctTaskBase
    {
        private readonly IFileSystem fileSystem;
        private readonly IDictionary<string, IEnumerable<string>> links;
        private readonly ISymlinkCreator symlinkCreator;

        public LinkTask(ILogger logger, IDictionary<string, IEnumerable<string>> links)
            : this(logger, links, new FileSystem())
        {
        }

        public LinkTask(
            ILogger logger,
            IDictionary<string, IEnumerable<string>> links,
            IFileSystem fileSystem)
            : this(logger, links, fileSystem, CreateDefaultSymlinkCreator())
        {
        }

        public LinkTask(
            ILogger logger,
            IDictionary<string, IEnumerable<string>> links,
            IFileSystem fileSystem,
            ISymlinkCreator symlinkCreator)
            : base(logger)
        {
            this.links = links;
            this.fileSystem = fileSystem;
            this.symlinkCreator = symlinkCreator;
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
                    if (this.fileSystem.File.Exists(target))
                    {
                        this.CreateLink(link, target, LinkType.File);
                    }
                    else if (this.fileSystem.Directory.Exists(target))
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

        private static ISymlinkCreator CreateDefaultSymlinkCreator()
        {
            return Platform.CurrentPlatform switch
            {
                PlatformType.Windows => new WindowsSymlinkCreator(),
                PlatformType.Linux => new UnixSymlinkCreator(),
                _ => throw new NotImplementedException(),
            };
        }

        private void CreateLink(string linkPath, string targetPath, LinkType linkType)
        {
            if (this.fileSystem.File.Exists(linkPath))
            {
                this.fileSystem.File.Delete(linkPath);
            }
            else if (this.fileSystem.Directory.Exists(linkPath))
            {
                this.fileSystem.Directory.Delete(linkPath);
            }

            string destinationLinkDirectory = linkPath[..linkPath.LastIndexOf(this.fileSystem.Path.DirectorySeparatorChar)];
            if (!this.fileSystem.Directory.Exists(destinationLinkDirectory))
            {
                this.fileSystem.Directory.CreateDirectory(destinationLinkDirectory);
            }

            this.symlinkCreator.CreateSymlink(linkPath, targetPath, linkType, this.Logger);
        }
    }
}
