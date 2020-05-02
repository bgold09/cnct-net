using System.Threading.Tasks;

namespace Cnct.Core.Configuration
{
    public interface ICnctActionSpec
    {
        string ActionType { get; }

        void Validate();

        Task ExecuteAsync(ILogger logger, string configDirectoryRoot);
    }
}
