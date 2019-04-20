﻿using System.Collections.Generic;
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

        public async Task ExecuteAsync()
        {
            var d = new Dictionary<string, IEnumerable<string>>();
            foreach (var kvp in this.Links)
            {
                string target = kvp.Key;

                IEnumerable<string> destinationPaths;
                var v = kvp.Value;
                switch (v)
                {
                    case null:
                        destinationPaths = new[] { $"{Platform.Home}/.{Path.GetFileName(target)}" };
                        break;

                    case string s:
                        destinationPaths = new[] { s };
                        break;

                    case SymlinkSpecification spec:
                        var dl = new List<string>();
                        if (spec.Windows != null)
                        {
                            dl.Add(spec.Windows);
                        }

                        if (spec.Linux != null)
                        {
                            dl.Add(spec.Linux);
                        }

                        if (spec.Osx != null)
                        {
                            dl.Add(spec.Osx);
                        }

                        destinationPaths = dl;
                        break;

                    default:
                        destinationPaths = null;
                        break;
                }

                d.Add(target, destinationPaths);
            }

            var l = new LinkTask(d);

            await l.ExecuteAsync();
        }
    }
}