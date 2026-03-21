using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    public interface IGitRunner
    {
        Task CloneAsync(string url, string destination);

        Task PullAsync(string repositoryPath);
    }
}
