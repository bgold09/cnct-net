using System.IO.Abstractions;

namespace Cnct.Core.Configuration
{
    public class PathResolver : IPathResolver
    {
        private readonly IFileSystem fileSystem;

        public PathResolver()
            : this(new FileSystem())
        {
        }

        public PathResolver(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public string Resolve(string path, string configDirectoryRoot)
        {
            string result = path.NormalizePath();
            if (!this.fileSystem.Path.IsPathRooted(result))
            {
                result = this.fileSystem.Path.Combine(configDirectoryRoot, result);
            }

            return result;
        }
    }
}
