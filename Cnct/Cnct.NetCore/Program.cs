using System.Threading.Tasks;
using Cnct.Core;

namespace Cnct
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await CnctCommandLine.Invoke(args);
        }
    }
}
