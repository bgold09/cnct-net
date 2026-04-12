using System.Text;

namespace Cnct.Core
{
    public interface ILogger
    {
        void LogInformation(string message);

        void LogVerbose(string message);

        void LogWarning(string message);

        void LogError(string message, Exception exception = null);

        void LogStart(string message);

        void LogFinish(string message);
    }
}
