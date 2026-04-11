namespace Cnct.Core.Tasks
{
    public interface IGitRunnerFactory
    {
        IGitRunner Create(ILogger logger);
    }
}
