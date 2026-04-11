using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cnct.Core.Configuration
{
    public class FileManagement : IFileManagement
    {
        private readonly IPathResolver pathResolver;
        private readonly IPlatformPathSelector platformPathSelector;

        public FileManagement()
            : this(new PathResolver(), new PlatformPathSelector())
        {
        }

        public FileManagement(IPathResolver pathResolver)
            : this(pathResolver, new PlatformPathSelector())
        {
        }

        public FileManagement(
            IPathResolver pathResolver,
            IPlatformPathSelector platformPathSelector)
        {
            this.pathResolver = pathResolver;
            this.platformPathSelector = platformPathSelector;
        }

        public IDictionary<string, IEnumerable<string>> GetFileConfigurations(
            string configDirectoryRoot,
            IReadOnlyDictionary<string, object> fileConfigs)
        {
            var fileCopyConfigs = new Dictionary<string, IEnumerable<string>>();
            foreach (var kvp in fileConfigs)
            {
                string sourceFile = this.pathResolver.Resolve(kvp.Key, configDirectoryRoot);
                object destination = kvp.Value;
                switch (destination)
                {
                    case null:
                        fileCopyConfigs.Add(sourceFile, new[] { GetDotFileLinkPath(sourceFile) });
                        break;

                    case string s:
                        fileCopyConfigs.Add(sourceFile, new[] { s.NormalizePath() });
                        break;

                    case FileSpecification spec:
                        string[] platformLinkPaths =
                            this.platformPathSelector.GetPlatformPaths(spec);

                        string[] destinationPaths;
                        if (TryGetPlatformLinkPaths(sourceFile, platformLinkPaths, out destinationPaths))
                        {
                            fileCopyConfigs.Add(sourceFile, destinationPaths);
                        }

                        if (this.platformPathSelector.IsCurrentPlatformUnix
                            && TryGetPlatformLinkPaths(
                                sourceFile,
                                this.platformPathSelector.GetUnixPaths(spec),
                                out destinationPaths))
                        {
                            fileCopyConfigs.Add(sourceFile, destinationPaths);
                        }

                        break;
                }
            }

            return fileCopyConfigs;
        }

        private static bool TryGetPlatformLinkPaths(string sourceFile, string[] platformDestinations, out string[] destinations)
        {
            if (platformDestinations == null)
            {
                destinations = null;
                return false;
            }
            else if (platformDestinations.Length == 0)
            {
                destinations = new[] { GetDotFileLinkPath(sourceFile) };
                return true;
            }
            else
            {
                destinations = platformDestinations.Select(p => p.NormalizePath()).ToArray();
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
