using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public sealed class LinkTaskSpecification : ICnctActionSpec
    {
        public string ActionType => "link";

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
                        string[] destinationPaths = null;
                        switch (Platform.CurrentPlatform)
                        {
                            case PlatformType.Windows:
                                destinationPaths = GetPlatformLinkPaths(target, spec.Windows);
                                break;
                            case PlatformType.Linux:
                                destinationPaths = GetPlatformLinkPaths(target, spec.Linux);
                                break;
                            case PlatformType.OSX:
                                destinationPaths = GetPlatformLinkPaths(target, spec.Osx);
                                break;
                        }

                        if (destinationPaths != null)
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

        private static string[] GetPlatformLinkPaths(string target, string[] platformLinkPaths)
        {
            if (platformLinkPaths == null)
            {
                return null;
            }
            else if (platformLinkPaths.Length == 0)
            {
                return new[] { GetDotFileLinkPath(target) };
            }
            else
            {
                return platformLinkPaths.Select(p => p.NormalizePath()).ToArray();
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
