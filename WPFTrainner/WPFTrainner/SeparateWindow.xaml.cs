using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ITrainerExtension;

namespace PVZWPFTrainner
{
    public partial class SeparateWindow : Window
    {
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (System.InvalidOperationException) { }
        }

        public int scale = 100;

        private void Window_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (e.Delta > 0) scale += 5; else scale -= 5;
                scale = System.Math.Max(10, scale);
                scale = System.Math.Min(300, scale);
                System.Windows.UIElement con = Content as System.Windows.UIElement;
                con.RenderTransform = new System.Windows.Media.ScaleTransform(scale / 100.0, scale / 100.0);
                Height = 470.0 * scale / 100;
                Width = 400.0 * scale / 100;
            }
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Lang.ChangeLanguage(Content);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var item = (System.Windows.Controls.TabItem)TCMain.Items[0];
            TCMain.Items.RemoveAt(0);
            var tcicollection = ((System.Windows.Controls.TabControl)item.Tag).Items;
            tcicollection.Add(item);
            tcicollection.SortDescriptions.Clear();
            tcicollection.SortDescriptions.Add(new SortDescription("TabIndex", ListSortDirection.Ascending));
            tcicollection.Refresh();
        }
    }
}
