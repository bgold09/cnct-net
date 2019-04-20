using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal interface ICnctTask
    {
        Task ExecuteAsync();
    }
}
