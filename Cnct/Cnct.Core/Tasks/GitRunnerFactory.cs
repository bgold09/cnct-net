namespace Cnct.Core.Tasks
{
    public class GitRunnerFactory : IGitRunnerFactory
    {
        public IGitRunner Create(ILogger logger) =>
            new ProcessGitRunner(logger);
    }
}
