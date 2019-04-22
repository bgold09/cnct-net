using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Newtonsoft.Json;

namespace Cnct.Core.Configuration
{
    public sealed class LinkTaskSpecification : ICnctActionSpec
    {
        public string ActionType => "link";

        [JsonConverter(typeof(LinkSpecificationCollectionConverter))]
        public IDictionary<string, object> Links { get; set; }

        public void Validate()
        {
            if (this.Links == null || this.Links.Count == 0)
            {
                throw new System.Exception();
            }
        }

        public IDictionary<string, IEnumerable<string>> GetLinkConfigurations()
        {
            var linkConfigs = new Dictionary<string, IEnumerable<string>>();
            foreach (var kvp in this.Links)
            {
                string target = kvp.Key;
                object linkValue = kvp.Value;
                switch (linkValue)
                {
                    case null:
                        linkConfigs.Add(target, new[] { NormalizeLink(target) });
                        break;

                    case string s:
                        linkConfigs.Add(target, new[] { s });
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

        public async Task ExecuteAsync(ILogger logger)
        {
            var linkTask = new LinkTask(logger, this.GetLinkConfigurations());
            await linkTask.ExecuteAsync();
        }

        private static string[] GetPlatformLinkPaths(string target, string[] platformLinkPaths)
        {
            if (platformLinkPaths == null)
            {
                return null;
            }

            return platformLinkPaths.Length == 0
                ? new[] { NormalizeLink(target) }
                : platformLinkPaths;
        }

        private static string NormalizeLink(string path)
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
