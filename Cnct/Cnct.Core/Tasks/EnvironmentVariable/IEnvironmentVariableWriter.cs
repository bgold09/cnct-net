namespace Cnct.Core.Tasks.EnvironmentVariable
{
    public interface IEnvironmentVariableWriter
    {
        void SetVariable(string name, string value);
    }
}
