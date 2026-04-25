namespace Cnct.Core.Configuration
{
    public sealed class CnctContext
    {
        public CnctContext(string configDirectoryRoot, IReadOnlyCollection<string> machineTags)
        {
            this.ConfigDirectoryRoot = configDirectoryRoot;
            this.MachineTags = machineTags ?? [];
        }

        public string ConfigDirectoryRoot { get; }

        public IReadOnlyCollection<string> MachineTags { get; }
    }
}
