using System.Threading.Tasks;
using Cnct.Core;

namespace Cnct
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CommandLineOptions.T(args);
        }
    }
}
