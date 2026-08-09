using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ITrainerExtension;
using PVZClass;

namespace PVZWPFTrainner
{
    public static class ControlExtension
    {
        public static System.Windows.Rect GetTextRect(this System.Windows.Controls.Control con, string str)
        {
            var typeface = new System.Windows.Media.Typeface(con.FontFamily, con.FontStyle, con.FontWeight, con.FontStretch);
            var formattedText = new System.Windows.Media.FormattedText(
                str, CultureInfo.CurrentUICulture, System.Windows.FlowDirection.LeftToRight, typeface, con.FontSize, con.Foreground);
            return new System.Windows.Rect(0, 0, formattedText.Width, formattedText.Height);
        }

        public static System.Windows.Rect GetTextRect(this System.Windows.Controls.TextBlock tb)
        {
            var typeface = new System.Windows.Media.Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch);
            var formattedText = new System.Windows.Media.FormattedText(
                tb.Text, CultureInfo.CurrentUICulture, System.Windows.FlowDirection.LeftToRight, typeface, tb.FontSize, tb.Foreground);
            return new System.Windows.Rect(0, 0, formattedText.Width, formattedText.Height);
        }
    }

    public class MyCheckBox : DarkStyle.DarkCheckBox
    {
        bool isTipSet = false;

        public MyCheckBox()
        {
            Click += MyCheckBox_Click;
            Loaded += MyCheckBox_Load;
        }

        private void MyCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsChecked.HasValue)
                Foreground = System.Windows.Media.Brushes.White;
            else
                Foreground = System.Windows.Media.Brushes.Red;
            if (Tag != null)
                typeof(PVZ).GetMethod(Tag.ToString())?.Invoke(null, new object[] { IsChecked });
        }

        string[] CBoxText1 = { "变红代表取消或变化原有的效果", "Reddening means canceling or changing the original effect" };
        string[] CBoxText2 = { "附带右键菜单额外功能", "Additional functions of right-click menu" };

        private void MyCheckBox_Load(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isTipSet) return;
            bool extra = Content != null && Content.ToString().Contains("*");
            if (ToolTip == null)
            {
                var tip = new MyToolTip();
                if (IsThreeState && !extra)
                {
                    tip.Content = CBoxText1[Lang.Id];
                    tip.Resources.Add("Lang", CBoxText1);
                }
                else if (extra && !IsThreeState)
                {
                    tip.Content = CBoxText2[Lang.Id];
                    tip.Resources.Add("Lang", CBoxText2);
                }
                else if (IsThreeState && extra)
                {
                    tip.Content = CBoxText1[Lang.Id] + "," + CBoxText2[Lang.Id];
                    string[] res = { CBoxText1[0] + "," + CBoxText2[0], CBoxText1[1] + "," + CBoxText2[1] };
                    tip.Resources.Add("Lang", res);
                }
                else return;
                ToolTip = tip;
                isTipSet = true;
            }
        }
    }

    public class MySlider : System.Windows.Controls.Slider
    {
        public MySlider()
        {
            Maximum = 20;
            AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.BottomRight;
            Width = 80;
        }
    }

    public class MyComboBox : DarkStyle.DarkComboBox
    {
        public MyComboBox()
        {
            Width = 75;
            Loaded += MyComboBox_Load;
        }

        private void MyComboBox_Load(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Tag != null && Items.Count == 0)
            {
                string[] tEnum = Tag.ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                Type enumType;
                if (tEnum.Length == 1)
                    enumType = (Type)typeof(PVZ).GetMember(tEnum[0])[0];
                else
                    enumType = (Type)typeof(PVZ).GetNestedType(tEnum[0]).GetMember(tEnum[1])[0];
                foreach (Enum value in Enum.GetValues(enumType))
                {
                    var item = new DarkStyle.DarkComboBoxItem();
                    string[] res = { value.GetDescription(), value.ToString() };
                    item.Resources.Add("Lang", res);
                    item.Content = ((string[])item.Resources["Lang"])[Lang.Id];
                    Items.Add(item);
                }
            }
        }
    }

    public class NegateConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(bool))
                return !(bool)value;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(bool))
                return !(bool)value;
            return value;
        }
    }

    public class IndexValueConverter : System.Windows.Data.IValueConverter
    {
        public int IndexValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(int))
                return IndexValue == (int)value;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(int))
                return IndexValue == (int)value;
            return true;
        }
    }

    public class MyToolTip : System.Windows.Controls.ToolTip
    {
        public MyToolTip()
        {
            Foreground = System.Windows.Media.Brushes.White;
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x25, 0x25, 0x26));
            BorderBrush = System.Windows.Media.Brushes.Crimson;
            Loaded += MyToolTip_Loaded;
        }

        void MyToolTip_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var win = System.Windows.Window.GetWindow(this);
            var scaleinfo = typeof(System.Windows.Window).GetField("scale", BindingFlags.NonPublic | BindingFlags.Instance);
            if (win != null)
                scaleinfo = win.GetType().GetField("scale");
            var openAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(300)));
            if (scaleinfo != null)
            {
                int scaleVal = Convert.ToInt32(scaleinfo.GetValue(win));
                if (scaleVal >= 200)
                {
                    FontSize = 22;
                    BorderThickness = new System.Windows.Thickness(3);
                }
                else if (scaleVal >= 150)
                {
                    FontSize = 18;
                    BorderThickness = new System.Windows.Thickness(2);
                }
                else
                {
                    FontSize = 12;
                    BorderThickness = new System.Windows.Thickness(1);
                }
            }
            var rect = this.GetTextRect(Content.ToString());
            RenderTransform = new System.Windows.Media.ScaleTransform(0, 0, rect.Width / 2, rect.Height / 2);
            RenderTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, openAnim);
            RenderTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, openAnim);
        }
    }
}
