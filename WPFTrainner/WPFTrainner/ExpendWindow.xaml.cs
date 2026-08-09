using System.Globalization;

namespace PVZWPFTrainner
{
    public partial class ExpendWindow : System.Windows.Window
    {
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.GetPosition(this).Y < 35)
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                    DragMove();
            }
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TBEgg.Text =
"---------[>--------<+]>---.<------[>------<+]>. \n" +
"++.++++++++.----.-.+.<--------[>>----<<+]>>.>-- \n" +
"---[<-------->+]<---.<------.++++++.<-----[>--< \n" +
"+]>.<-----[>++<+]>+.-.>>------[<+++++>+]<+.>--- \n" +
"---[<----->+]<+.>--------[<+++++>+]<+.<---.---. \n" +
"-------.<-----[>+++<+]>++.>.<<----[>-----<+]>.< \n" +
"--[>+++++<+]>.------.>-...>------[<++++>+]<-.";
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (Tag != null)
                ((System.Windows.Controls.ListBoxItem)Tag).IsEnabled = true;
        }

        [System.Obsolete]
        private void Window_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            TBSep.Text = "";
            while (GetTextDisplayWidthHelper.GetTextDisplayWidth(TBSep) + System.Windows.Controls.Canvas.GetLeft(TBSep) < e.NewSize.Width - 15)
                TBSep.Text += "-";
        }
    }

    public class GetTextDisplayWidthHelper
    {
        [System.Obsolete]
        public static double GetTextDisplayWidth(System.Windows.Controls.TextBlock textblock)
        {
            return GetTextDisplayWidth(textblock.Text, textblock.FontFamily, textblock.FontStyle, textblock.FontWeight, textblock.FontStretch, textblock.FontSize);
        }

        [System.Obsolete]
        public static double GetTextDisplayWidth(string str, System.Windows.Media.FontFamily fontFamily, System.Windows.FontStyle fontStyle, System.Windows.FontWeight fontWeight, System.Windows.FontStretch fontStretch, double FontSize)
        {
            var formattedText = new System.Windows.Media.FormattedText(
                str,
                CultureInfo.CurrentUICulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                FontSize,
                System.Windows.Media.Brushes.Black
            );
            var s = new System.Windows.Size(formattedText.Width, formattedText.Height);
            return s.Width;
        }
    }
}
