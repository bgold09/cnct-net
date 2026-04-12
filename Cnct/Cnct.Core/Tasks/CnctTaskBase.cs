namespace Cnct.Core.Tasks
{
    public abstract class CnctTaskBase : ICnctTask
    {
        protected ILogger Logger { get; }

        protected CnctTaskBase(ILogger logger)
        {
            this.Logger = logger;
        }

        public abstract Task ExecuteAsync();
    }
}
