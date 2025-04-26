using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cnct.Core.Configuration
{
    public class FileManagement : IFileManagement
    {
        public IDictionary<string, IEnumerable<string>> GetFileConfigurations(
            string configDirectoryRoot,
            IReadOnlyDictionary<string, object> fileConfigs)
        {
            var fileCopyConfigs = new Dictionary<string, IEnumerable<string>>();
            foreach (var kvp in fileConfigs)
            {
                string sourceFile = PathExtensions.NormalizePath($"{configDirectoryRoot}{Path.DirectorySeparatorChar}{kvp.Key}");
                object destination = kvp.Value;
                switch (destination)
                {
                    case null:
                        fileCopyConfigs.Add(sourceFile, new[] { GetDotFileLinkPath(sourceFile) });
                        break;

                    case string s:
                        fileCopyConfigs.Add(sourceFile, new[] { s.NormalizePath() });
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
                        if (TryGetPlatformLinkPaths(sourceFile, platformLinkPaths, out destinationPaths))
                        {
                            fileCopyConfigs.Add(sourceFile, destinationPaths);
                        }

                        if (Platform.CurrentPlatformIsUnix && TryGetPlatformLinkPaths(sourceFile, spec.Unix, out destinationPaths))
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
