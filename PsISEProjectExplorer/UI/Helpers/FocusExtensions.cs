using System.Windows;
using System.Windows.Controls;

namespace PsISEProjectExplorer.UI.Helpers
{
    // http://stackoverflow.com/questions/5152324/transfer-focus-on-a-controls-sub-part-in-its-template-in-wpf
    public static class FocusExtensions
    {
        public static bool GetIsDefaultFocusElement(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDefaultFocusElementProperty);
        }

        public static void SetIsDefaultFocusElement(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDefaultFocusElementProperty, value);
        }

        public static readonly DependencyProperty IsDefaultFocusElementProperty =
            DependencyProperty.RegisterAttached("IsDefaultFocusElement", typeof(bool), typeof(FocusExtensions), new UIPropertyMetadata(false, OnIsDefaultFocusElementChanged));

        private static void OnIsDefaultFocusElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)d;

            if (!(bool)e.NewValue)
            {
                return;
            }

            if (fe.IsLoaded)
            {
                SetFocus(fe);
            }
            else
            {
                fe.Loaded += OnDefaultFocusElementLoaded;
            }
        }

        private static void OnDefaultFocusElementLoaded(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement)sender;

            fe.Loaded -= OnDefaultFocusElementLoaded;

            SetFocus(fe);
        }

        private static void SetFocus(FrameworkElement element)
        {
            element.Focus();
            var textboxElement = element as TextBox;
            if (textboxElement != null)
            {
                int selectionEnd = textboxElement.Text.LastIndexOf('.');
                if (selectionEnd == -1) {
                    selectionEnd = textboxElement.Text.Length;
                }
                textboxElement.Select(0, selectionEnd);
            }
            
        }
    }
}
