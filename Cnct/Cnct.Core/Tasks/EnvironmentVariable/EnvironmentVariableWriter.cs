namespace Cnct.Core.Tasks.EnvironmentVariable
{
    public class EnvironmentVariableWriter : IEnvironmentVariableWriter
    {
        public void SetVariable(string name, string value) =>
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
    }
}
