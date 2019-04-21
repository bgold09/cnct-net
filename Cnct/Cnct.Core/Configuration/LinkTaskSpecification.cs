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

        public bool? Force { get; set; }

        [JsonConverter(typeof(LinkSpecificationCollectionConverter))]
        public IDictionary<string, object> Links { get; set; }

        public void Validate()
        {
            if (this.Links == null || this.Links.Count == 0)
            {
                throw new System.Exception();
            }
        }

        public async Task ExecuteAsync(ILogger logger)
        {
            var d = new Dictionary<string, IEnumerable<string>>();
            foreach (var kvp in this.Links)
            {
                string target = kvp.Key;

                var v = kvp.Value;
                switch (v)
                {
                    case null:
                        d.Add(target, new[] { Gen(target) });
                        break;

                    case string s:
                        d.Add(target, new[] { s });
                        break;

                    case SymlinkSpecification spec:
                        string[] destinationPaths = null;
                        switch (Platform.CurrentPlatform)
                        {
                            case PlatformType.Windows:
                                destinationPaths = A(target, spec.Windows);
                                break;
                            case PlatformType.Linux:
                                destinationPaths = A(target, spec.Linux);
                                break;
                            case PlatformType.OSX:
                                destinationPaths = A(target, spec.Osx);
                                break;
                        }

                        if (destinationPaths != null)
                        {
                            d.Add(target, destinationPaths);
                        }

                        break;
                }
            }

            var l = new LinkTask(logger, d);
            await l.ExecuteAsync();
        }

        private static string[] A(string target, string[] d)
        {
            if (d == null)
            {
                return null;
            }

            return d.Length == 0
                ? new[] { Gen(target) }
                : d;
        }

        private static string Gen(string p)
        {
            string fileName = Path.GetFileName(p);
            if (fileName[0] != '.')
            {
                fileName = $".{fileName}";
            }

            return $"{Platform.Home}{Path.DirectorySeparatorChar}{fileName}";
        }
    }
}
