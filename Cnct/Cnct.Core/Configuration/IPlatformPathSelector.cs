namespace Cnct.Core.Configuration
{
    public interface IPlatformPathSelector
    {
        string[] GetPlatformPaths(FileSpecification spec);

        string[] GetUnixPaths(FileSpecification spec);

        bool IsCurrentPlatformUnix { get; }
    }
}
