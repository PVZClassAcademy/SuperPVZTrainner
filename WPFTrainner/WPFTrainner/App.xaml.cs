using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ITrainerExtension;

namespace PVZWPFTrainner
{
    public partial class App : Application
    {
        public static bool IsChineseSystem()
        {
            string lang = Thread.CurrentThread.CurrentCulture.Name;
            return lang.StartsWith("zh");
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!IsChineseSystem())
                Lang.Id = 1;
        }

        public void _InitializeComponent()
        {
            InitializeComponent();
        }

        public static void SendToAuthor(string title, string message)
        {
            string msg = string.Format("mailto:lazuplismei@163.com?subject={0}&body={1}", title, message);
            Process.Start(msg);
        }

        [STAThread]
        [Obsolete]
        public static void Main()
        {
            AppDomain.CurrentDomain.AppendPrivatePath("Extension");
            var app = new App();
            app.InitializeComponent();
            if (Debugger.IsAttached)
            {
                app.Run();
            }
            else
            {
                try
                {
                    app.Run();
                }
                catch (Exception ex)
                {
                    var output = new ExpendWindow();
                    output.TBTitle.Text = ex.Message.Replace(Environment.NewLine, "");
                    if (output.TBTitle.GetTextRect().Width > 480)
                    {
                        while (output.TBTitle.GetTextRect().Width > 470)
                            output.TBTitle.Text = output.TBTitle.Text.Substring(0, output.TBTitle.Text.Length - 1);
                        output.TBTitle.Text += "...";
                    }
                    var text = new TextBox
                    {
                        Width = 578,
                        Height = 295,
                        Background = System.Windows.Media.Brushes.Transparent,
                        Foreground = System.Windows.Media.Brushes.White,
                        IsReadOnly = true,
                        TextWrapping = TextWrapping.Wrap
                    };
                    Canvas.SetTop(text, 56);
                    Canvas.SetLeft(text, 10);
                    text.Text = ex.ToString();
                    var btn = new DarkStyle.DarkButton
                    {
                        Width = 200,
                        Content = Lang.Id == 1 ? "Restart" : "重启程序",
                        FontSize = 20
                    };
                    Canvas.SetBottom(btn, 10);
                    Canvas.SetLeft(btn, 80);
                    btn.Click += (s, ev) =>
                    {
                        System.Windows.Forms.Application.Restart();
                        app.Shutdown();
                    };
                    output.MainCanvas.Children.Add(btn);
                    btn = new DarkStyle.DarkButton
                    {
                        Width = 200,
                        Content = Lang.Id == 1 ? "SendToAuthor" : "发给作者",
                        FontSize = 20
                    };
                    Canvas.SetBottom(btn, 10);
                    Canvas.SetRight(btn, 80);
                    btn.Click += (s, ev) =>
                    {
                        var input = new InputDialog(
                            Lang.Id == 1 ? "Are you sure to send an email?" : "确认要发送邮件?(万一得到回复了呢?)",
                            Lang.Id == 1 ? "Your Tencent QQ number(if you have)" : "请输入您的QQ号",
                            1, 99999999999);
                        if (input.ShowDialog() == true)
                            SendToAuthor($"[{input.Value.ToString()}]" + ex.Message.Replace(Environment.NewLine, ""), ex.ToString());
                        app.Shutdown();
                    };
                    output.MainCanvas.Children.Add(btn);
                    output.MainCanvas.Children.Add(text);
                    output.ShowDialog();
                }
            }
        }
    }
}
