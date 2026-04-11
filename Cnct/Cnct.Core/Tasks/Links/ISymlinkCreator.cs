namespace Cnct.Core.Tasks
{
    internal interface ISymlinkCreator
    {
        void CreateSymlink(string linkPath, string targetPath, LinkType linkType, ILogger logger);
    }
}
