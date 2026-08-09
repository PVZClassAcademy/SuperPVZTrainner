using System.Windows;
using System.Windows.Input;

namespace PVZWPFTrainner
{
    public partial class InputDialog : Window
    {
        public double Value { get; set; }

        public InputDialog(string title, string desc, long minval = 0, long maxval = 1000)
        {
            InitializeComponent();
            TBTitle.Text = title;
            NudInput.MinValue = minval;
            NudInput.MaxValue = maxval;
            if (ITrainerExtension.Lang.Id == 1)
            {
                TBDesc.Text = desc + $"Range({minval}-{maxval})";
                BtnOK.Content = "Ok";
                BtnCancel.Content = "Cancel";
            }
            else
            {
                TBDesc.Text = desc + $"范围({minval}-{maxval})";
                BtnOK.Content = "确认";
                BtnCancel.Content = "取消";
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (System.InvalidOperationException) { }
        }

        private void MyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Value = NudInput.Value;
            DialogResult = true;
        }

        private void MyButton_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
                Height = 180.0 * scale / 100;
                Width = 400.0 * scale / 100;
            }
        }
    }
}
