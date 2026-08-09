using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using PVZClass;

namespace PVZWPFTrainner
{
    public partial class HpTrackWindow : Window
    {
        public System.Windows.Media.Color hpfontcolor = System.Windows.Media.Colors.White;

        private struct Spoint
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hwnd, ref Spoint lppoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hwnd);

        private int prezombienum = 0;
        private System.Windows.Threading.DispatcherTimer Timer;

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IsHitTestVisible = false;
            dpi = GetDpiFromVisual();
            SetWindowLong(new WindowInteropHelper(this).Handle, -20, 0x20);
            Timer = new System.Windows.Threading.DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(30);
            Timer.Tick += TimerTick;
            Timer.Start();
        }

        double dpi;

        private double GetDpiFromVisual()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
                return source.CompositionTarget.TransformToDevice.M11;
            return 1;
        }

        public bool IsHide { get; set; }

        private void TimerTick(object sender, EventArgs e)
        {
            var p = new Spoint();
            if (PVZ.Game != null)
            {
                ClientToScreen(PVZ.Game.MainWindowHandle, ref p);
                if (!IsHide)
                {
                    if (IsIconic(PVZ.Game.MainWindowHandle))
                        Visibility = System.Windows.Visibility.Collapsed;
                    else
                        Visibility = System.Windows.Visibility.Visible;
                }
            }
            Top = p.y / dpi;
            Left = p.x / dpi;
            Height = 600.0 / dpi;
            Width = 800.0 / dpi;
            if (PVZ.ZombiesCount != prezombienum)
            {
                Canvas1.Children.Clear();
                foreach (var zombie in PVZ.AllZombies)
                {
                    var tb = new System.Windows.Controls.TextBlock();
                    tb.Foreground = new System.Windows.Media.SolidColorBrush(hpfontcolor);
                    tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    tb.FontWeight = System.Windows.FontWeights.Bold;
                    tb.Margin = new System.Windows.Thickness(3, 0, 3, 0);
                    tb.IsHitTestVisible = false;
                    var b = new System.Windows.Controls.Border();
                    b.BorderBrush = new System.Windows.Media.SolidColorBrush(hpfontcolor);
                    b.Child = tb;
                    Canvas1.Children.Add(b);
                }
                prezombienum = PVZ.ZombiesCount;
            }
            var zblist = PVZ.AllZombies;
            var blist = Canvas1.Children.Cast<System.Windows.Controls.Border>();
            var tblist = new System.Collections.Generic.List<System.Windows.Controls.TextBlock>();
            foreach (var b in blist)
                tblist.Add(b.Child as System.Windows.Controls.TextBlock);
            if (zblist.Length == tblist.Count)
            {
                for (int i = 0; i < zblist.Length; i++)
                {
                    tblist[i].Text = "";
                    ((System.Windows.Controls.Border)tblist[i].Parent).BorderThickness = new System.Windows.Thickness(0);
                    if (zblist[i].BodyHP != zblist[i].MaxBodyHP && zblist[i].BodyHP != 0)
                    {
                        tblist[i].Text += zblist[i].BodyHP.ToString();
                        tblist[i].Text += "/";
                        tblist[i].Text += zblist[i].MaxBodyHP.ToString();
                        ((System.Windows.Controls.Border)tblist[i].Parent).BorderThickness = new System.Windows.Thickness(1);
                    }
                    if (zblist[i].AccessoriesType1HP != zblist[i].MaxAccessoriesType1HP && zblist[i].AccessoriesType1HP != 0)
                    {
                        if (zblist[i].BodyHP != zblist[i].MaxBodyHP)
                            tblist[i].Text += Environment.NewLine;
                        tblist[i].Text += zblist[i].AccessoriesType1HP.ToString();
                        tblist[i].Text += "/";
                        tblist[i].Text += zblist[i].MaxAccessoriesType1HP.ToString();
                        ((System.Windows.Controls.Border)tblist[i].Parent).BorderThickness = new System.Windows.Thickness(1);
                    }
                    if (zblist[i].AccessoriesType2HP != zblist[i].MaxAccessoriesType2HP && zblist[i].AccessoriesType2HP != 0)
                    {
                        if (zblist[i].BodyHP != zblist[i].MaxBodyHP)
                            tblist[i].Text += Environment.NewLine;
                        if (zblist[i].AccessoriesType1HP != zblist[i].MaxAccessoriesType1HP)
                            tblist[i].Text += Environment.NewLine;
                        tblist[i].Text += zblist[i].AccessoriesType2HP.ToString();
                        tblist[i].Text += "/";
                        tblist[i].Text += zblist[i].MaxAccessoriesType2HP.ToString();
                        ((System.Windows.Controls.Border)tblist[i].Parent).BorderThickness = new System.Windows.Thickness(1);
                    }
                    Canvas1.Children[i].SetValue(System.Windows.Controls.Canvas.LeftProperty, Convert.ToDouble(zblist[i].ImageX + 10) / dpi);
                    Canvas1.Children[i].SetValue(System.Windows.Controls.Canvas.TopProperty, Convert.ToDouble(zblist[i].ImageY - 10) / dpi);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Timer?.Stop();
        }
    }
}
