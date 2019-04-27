using System;
using System.IO;
using Cnct.Core.Configuration;

namespace Cnct.Core
{
    public static class PathExtensions
    {
        /// <summary>
        /// Expand environment variables and replace directory separators with platform-specific separators.
        /// </summary>
        /// <param name="path">The file or directory path to normalize.</param>
        /// <returns>A normalized version of <paramref name="path"/>.</returns>
        public static string NormalizePath(this string path)
        {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return path.StartsWith("~", StringComparison.Ordinal)
                ? path.Replace("~", Platform.Home)
                : path;
        }
    }
}
