namespace Cnct.Core.Tasks.Shell
{
    public class ShellExecutionOptions
    {
        public ShellExecutionOptions(
            string command, bool silent)
        {
            this.Command = command ?? throw new ArgumentNullException(nameof(command));
            this.Silent = silent;
        }

        public string Command { get; }

        public bool Silent { get; }
    }
}
