using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class ClearSearchTextCommand : Command
    {
        private MainViewModel mainViewModel;

        public ClearSearchTextCommand(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }

        public void Execute()
        {
            this.mainViewModel.SearchText = string.Empty;
        }

    }
}
