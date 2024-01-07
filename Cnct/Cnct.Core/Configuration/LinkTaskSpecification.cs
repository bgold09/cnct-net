using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    [CnctActionType("link")]
    public sealed partial class LinkTaskSpecification : ICnctActionSpec
    {
        [JsonConverter(typeof(LinkSpecificationCollectionConverter))]
        public IReadOnlyDictionary<string, object> Links { get; set; }

        public void Validate()
        {
            if (this.Links == null || this.Links.Count == 0)
            {
                throw new InvalidOperationException("The collection of links cannot be null or empty.");
            }
        }

        public IDictionary<string, IEnumerable<string>> GetLinkConfigurations(string configDirectoryRoot)
        {
            var linkConfigs = new Dictionary<string, IEnumerable<string>>();
            foreach (var kvp in this.Links)
            {
                string target = PathExtensions.NormalizePath($"{configDirectoryRoot}{Path.DirectorySeparatorChar}{kvp.Key}");
                object linkValue = kvp.Value;
                switch (linkValue)
                {
                    case null:
                        linkConfigs.Add(target, new[] { GetDotFileLinkPath(target) });
                        break;

                    case string s:
                        linkConfigs.Add(target, new[] { s.NormalizePath() });
                        break;

                    case SymlinkSpecification spec:
                        string[] platformLinkPaths = Platform.CurrentPlatform switch
                        {
                            PlatformType.Windows => spec.Windows,
                            PlatformType.Linux => spec.Linux,
                            PlatformType.OSX => spec.Osx,
                            _ => throw new NotImplementedException(),
                        };

                        string[] destinationPaths;
                        if (TryGetPlatformLinkPaths(target, platformLinkPaths, out destinationPaths))
                        {
                            linkConfigs.Add(target, destinationPaths);
                        }

                        if (Platform.CurrentPlatformIsUnix && TryGetPlatformLinkPaths(target, spec.Unix, out destinationPaths))
                        {
                            linkConfigs.Add(target, destinationPaths);
                        }

                        break;
                }
            }

            return linkConfigs;
        }

        public async Task ExecuteAsync(ILogger logger, string confgiDirectoryRoot)
        {
            var linkTask = new LinkTask(logger, this.GetLinkConfigurations(confgiDirectoryRoot));
            await linkTask.ExecuteAsync();
        }

        private static bool TryGetPlatformLinkPaths(string target, string[] platformLinkPaths, out string[] destinationLinks)
        {
            if (platformLinkPaths == null)
            {
                destinationLinks = null;
                return false;
            }
            else if (platformLinkPaths.Length == 0)
            {
                destinationLinks = new[] { GetDotFileLinkPath(target) };
                return true;
            }
            else
            {
                destinationLinks = platformLinkPaths.Select(p => p.NormalizePath()).ToArray();
                return true;
            }
        }

        private static string GetDotFileLinkPath(string path)
        {
            string fileName = Path.GetFileName(path);
            if (fileName[0] != '.')
            {
                fileName = $".{fileName}";
            }

            return $"{Platform.Home}{Path.DirectorySeparatorChar}{fileName}";
        }
    }
}
