namespace PsISEProjectExplorer.Commands
{
    public interface ParameterizedCommand<T>
    {
        void Execute(T param);
    }
}
