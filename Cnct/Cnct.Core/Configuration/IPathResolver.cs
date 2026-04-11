namespace Cnct.Core.Configuration
{
    public interface IPathResolver
    {
        string Resolve(string path, string configDirectoryRoot);
    }
}
