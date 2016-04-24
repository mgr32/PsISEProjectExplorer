using System.Windows.Controls;
using System.Windows.Input;

namespace PsISEProjectExplorer.UI.Helpers
{
    public static class TreeViewSpecialCharactersHandler
    {
        public static void RouteSpecialCharacters(this TreeView treeView, object sender, KeyEventArgs e)
        {
            string keyText = null;
            switch (e.Key)
            {
                case Key.Subtract: keyText = "-"; break;
                case Key.Add: keyText = "+"; break;
                case Key.Multiply: keyText = "*"; break;
            }
            if (keyText == null)
            {
                return;
            }

            var target = Keyboard.FocusedElement;
            if (target == null)
            {
                return;
            }
            e.Handled = true;
            var routedEvent = TextCompositionManager.TextInputEvent;
            target.RaiseEvent(
                new TextCompositionEventArgs
                    (
                        InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, keyText)
                    )
                {
                    RoutedEvent = routedEvent
                });
        }
    }
}
