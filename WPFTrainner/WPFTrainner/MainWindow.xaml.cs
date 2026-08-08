using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ITrainerExtension;
using Microsoft.Win32;
using PVZClass;

namespace PVZWPFTrainner
{
    public partial class MainWindow : Window
    {
        public static int StrToInt(string Value)
        {
            try { return Convert.ToInt32(Value); }
            catch (OverflowException) { return int.MaxValue; }
            catch (InvalidCastException) { return 0; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            openFileDlg = new OpenFileDialog();
            openFileDlg.DefaultExt = ".dll";
            openFileDlg.Multiselect = true;
            LBMain.Tag = -1;
            ListPlugIns.Tag = -1;
            FindGame();
            if (Directory.Exists("Extension"))
            {
                foreach (string f in Directory.GetFiles("Extension", "*.dll"))
                    AddExtension(Path.GetFullPath(f));
            }
            if (Directory.Exists("Scripts"))
            {
                foreach (string f in Directory.GetFiles("Scripts", "*.pvzs"))
                    AddLBIScr(Path.GetFullPath(f));
            }
            Lang.ChangeLanguage(Content);
        }

        private void FindGame()
        {
            if (PVZ.RunGame())
            {
                var check = PVZ.CheckPeocess();
                if (check.HasValue)
                {
                    if (check.Value)
                    {
                        TBStatus.Text = StatusTextFound[Lang.Id];
                        PVZ.InitFunctions();
                        PVZ.Game.EnableRaisingEvents = true;
                        PVZ.Game.Exited += GameExited;
                    }
                    else
                    {
                        TBStatus.Text = StatusTextNotSuppost[Lang.Id];
                    }
                }
                else
                {
                    TBStatus.Text = StatusTextOpenFailed[Lang.Id];
                }
            }
            else
            {
                TBStatus.Text = StatusTextNotFound[Lang.Id];
            }
        }

        private void GameExited(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => TBStatus.Text = StatusTextNotFound[Lang.Id]);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PVZ.CloseGame();
            string temp = Environment.GetEnvironmentVariable("Temp");
            foreach (string f in Directory.GetFiles(temp, "PlantsVsZombies_Temp*.exe"))
            {
                try { File.Delete(f); } catch { continue; }
            }
            Application.Current.Shutdown();
            System.Windows.Forms.Application.Exit();
        }

        private void AddLBIScr(string scriptfile)
        {
            if (ChecckPlugIns(scriptfile)) return;
            string ALBIStr = File.ReadAllText(scriptfile);
            ALBIStr = ALBIStr.TrimEnd('\r', ' ', '\n');
            if (ALBIStr.EndsWith("End") || ALBIStr.EndsWith("EndScript"))
            {
                var Btn = new DarkStyle.DarkButton();
                string[] substr = scriptfile.Split('\\');
                string name = substr[substr.Length - 1];
                Btn.Content = name.Substring(0, name.Length - 5);
                Btn.Tag = scriptfile;
                Btn.Style = FindResource("LBIBtnnStyle1") as Style;
                Btn.Width = 175;
                Btn.Click += ButtonScript_Click;
                ListPlugIns.Items.Add(Btn);
            }
        }

        private void ButtonScript_Click(object sender, RoutedEventArgs e)
        {
            var pvzscript = new Process();
            pvzscript.StartInfo.FileName = "PVZScript.exe";
            pvzscript.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pvzscript.StartInfo.Arguments = "\"" + ((FrameworkElement)sender).Tag + "\"";
            try
            {
                pvzscript.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                if (Lang.Id == 1)
                    MessageBox.Show("The program PVZScriptNoConsole.exe was not found." + Environment.NewLine + "Please place the program in the PVZScript directory", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("没有找到程序PVZScriptNoConsole.exe,请将程序放置到PVZScript目录下", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool ChecckPlugIns(string file)
        {
            foreach (var lbi in ListPlugIns.Items)
            {
                if (((Control)lbi).Tag?.ToString() == file)
                {
                    if (Lang.Id == 1)
                        MessageBox.Show($"{file} is already added", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else
                        MessageBox.Show($"项目{file}已经添加", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return true;
                }
            }
            return false;
        }

        private void AddExtension(string extensionfile)
        {
            if (ChecckPlugIns(extensionfile)) return;
            Assembly asm;
            try { asm = Assembly.LoadFile(extensionfile); }
            catch (BadImageFormatException) { return; }
            var types = asm.GetExportedTypes();
            foreach (var type in types)
            {
                if (type.IsClass)
                {
                    var ifaces = type.GetInterfaces();
                    foreach (var iface in ifaces)
                    {
                        if (iface == typeof(ITrainerExtensionButton))
                            AddButtonPlugIn(extensionfile, type);
                        else if (iface == typeof(IITrainerExtensionCheckBox))
                            AddCheckBoxPlugIn(extensionfile, type);
                        else if (iface == typeof(ITrainerExtensionTextBox))
                            AddTextBoxPlugIn(extensionfile, type);
                        else if (iface == typeof(ITrainerExtensionUserControl))
                            AddUserControlPlugIn(extensionfile, type);
                    }
                }
            }
        }

        private void AddUserControlPlugIn(string extensionfile, Type type)
        {
            var usercon = (ITrainerExtensionUserControl)Activator.CreateInstance(type);
            var mi = new ListBoxItem
            {
                Content = usercon.Text,
                Resources = new ResourceDictionary { { "Lang", usercon.TextLang } },
                Tag = extensionfile,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            if (usercon.ToolTip != null)
            {
                mi.ToolTip = new MyToolTip
                {
                    Content = usercon.ToolTip,
                    Resources = new ResourceDictionary { { "Lang", usercon.ToolTipLang } }
                };
            }
            mi.MouseDoubleClick += (s, ev) =>
            {
                expanderPlugIns.IsExpanded = false;
                var expend = new ExpendWindow();
                expend.Tag = mi;
                expend.TBTitle.Text = usercon.Text;
                expend.TBTitle.Resources = new ResourceDictionary { { "Lang", usercon.TextLang } };
                usercon.Layout(expend, expend.MainCanvas);
                Lang.ChangeLanguage(expend.MainCanvas);
                expend.Show();
                mi.IsEnabled = false;
            };
            ListPlugIns.Items.Add(mi);
        }

        private void AddTextBoxPlugIn(string extensionfile, Type type)
        {
            var lbibtnwithcb = (ITrainerExtensionTextBox)Activator.CreateInstance(type);
            var TBlock = new TextBlock
            {
                Foreground = Brushes.White,
                Text = lbibtnwithcb.Text,
                Resources = new ResourceDictionary { { "Lang", lbibtnwithcb.TextLang } }
            };
            var TBox = new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0),
                Width = 125,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x25, 0x26)),
                AllowDrop = false,
                BorderThickness = new Thickness(0)
            };
            InputMethod.SetIsInputMethodEnabled(TBox, false);
            TBox.ContextMenu = null;
            TBox.PreviewKeyDown += TBSun_PreviewKeyDown;
            var lbi = new ListBoxItem();
            lbi.MouseDoubleClick += (s, ev) => lbibtnwithcb.FunctionOnCall(TBox.Text);
            var g = new Grid { Margin = new Thickness(0, 0, -4, 0) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(TBox, 1);
            g.Children.Add(TBlock);
            g.Children.Add(TBox);
            lbi.Content = g;
            lbi.Tag = extensionfile;
            if (lbibtnwithcb.ToolTip != null)
            {
                lbi.ToolTip = new MyToolTip
                {
                    Content = lbibtnwithcb.ToolTip,
                    Resources = new ResourceDictionary { { "Lang", lbibtnwithcb.ToolTipLang } }
                };
            }
            ListPlugIns.Items.Add(lbi);
        }

        private void AddCheckBoxPlugIn(string extensionfile, Type type)
        {
            var lbicheckbox = (IITrainerExtensionCheckBox)Activator.CreateInstance(type);
            var Cbox = new MyCheckBox
            {
                Content = lbicheckbox.Text,
                Resources = new ResourceDictionary { { "Lang", lbicheckbox.TextLang } },
                Tag = extensionfile,
                Style = FindResource("CheckBoxStyle1") as Style
            };
            if (lbicheckbox.ToolTip != null)
            {
                Cbox.ToolTip = new MyToolTip
                {
                    Content = lbicheckbox.ToolTip,
                    Resources = new ResourceDictionary { { "Lang", lbicheckbox.ToolTipLang } }
                };
            }
            Cbox.Click += (sender2, e2) =>
            {
                lbicheckbox.CheckBoxOnClick(((CheckBox)sender2).IsChecked == true);
            };
            ListPlugIns.Items.Add(Cbox);
        }

        private void AddButtonPlugIn(string extensionfile, Type type)
        {
            var lbibutton = (ITrainerExtensionButton)Activator.CreateInstance(type);
            var Btn = new DarkStyle.DarkButton
            {
                Content = lbibutton.Text,
                Resources = new ResourceDictionary { { "Lang", lbibutton.TextLang } },
                Tag = extensionfile,
                Style = FindResource("LBIBtnnStyle1") as Style,
                Width = 175
            };
            if (lbibutton.ToolTip != null)
            {
                Btn.ToolTip = new MyToolTip
                {
                    Content = lbibutton.ToolTip,
                    Resources = new ResourceDictionary { { "Lang", lbibutton.ToolTipLang } }
                };
            }
            Btn.Click += (s, e) => lbibutton.ButtonOnClick();
            ListPlugIns.Items.Add(Btn);
        }

        private void BtnLoadLBMain_Click(object sender, RoutedEventArgs e)
        {
            if (Lang.Id == 1)
                openFileDlg.Filter = "extension plugin|*.dll|pvz script file|*.pvzs";
            else
                openFileDlg.Filter = "扩展插件|*.dll|pvz脚本文件|*.pvzs";
            openFileDlg.Title = Lang.Id == 1 ? "Load PlugIn" : "载入插件";
            if (openFileDlg.ShowDialog() == true)
            {
                if (openFileDlg.FilterIndex == 1)
                {
                    foreach (string f in openFileDlg.FileNames)
                        AddExtension(f);
                }
                else if (openFileDlg.FilterIndex == 2)
                {
                    foreach (string f in openFileDlg.FileNames)
                        AddLBIScr(f);
                }
            }
        }

        private void LBMain_KeyDown(object sender, KeyEventArgs e)
        {
            var listbox = sender as ListBox;
            if (e.Key == Key.Delete && listbox.Name == "ListPlugIns")
                listbox.Items.Remove(listbox.SelectedItem);
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Key == Key.C)
                {
                    listbox.Tag = listbox.SelectedIndex;
                    try { Clipboard.SetText(((ListBox)listbox).Items.GetItemAt(Convert.ToInt32(((ListBox)listbox).Tag)).Tag?.ToString()); }
                    catch (ArgumentNullException) { Clipboard.SetText("+ <=> -"); }
                }
                if (e.Key == Key.V)
                {
                    if (Convert.ToInt32(listbox.Tag) != -1)
                    {
                        var temp = listbox.Items.GetItemAt(Convert.ToInt32(listbox.Tag));
                        listbox.Items.Remove(temp);
                        listbox.Items.Insert(listbox.SelectedIndex + 1, temp);
                        listbox.Tag = -1;
                    }
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (InvalidOperationException) { }
        }

        private void TBSun_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var tb = (TextBox)sender;
                if (tb.Parent is Grid)
                {
                    var grand = ((Grid)tb.Parent).Parent;
                    if (grand is ListBoxItem)
                    {
                        var dce = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = MouseDoubleClickEvent };
                        ((ListBoxItem)grand).RaiseEvent(dce);
                    }
                }
            }
            else if (e.Key == Key.Space)
            {
                TBSun.Text = "0";
                e.Handled = true;
            }
            else if (e.Key == Key.V)
            {
                e.Handled = true;
            }
        }

        private void LBISun_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int Value = StrToInt(TBSun.Text);
            PVZ.Sun = Value;
            TBSun.Text = Value.ToString();
        }

        private void LBIMoney_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int Value = (int)(StrToInt(TBMoney.Text) / 10);
            PVZ.SaveData.Money = Value;
            TBMoney.Text = (Value * 10).ToString();
        }

        private void LBISunMax_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int Value = StrToInt(TBSunMax.Text);
            PVZ.SunMax = Value;
            TBSunMax.Text = Value.ToString();
        }

        private void CBBGRunable_Click(object sender, RoutedEventArgs e) { PVZ.BGRunable(CBBGRunable.IsChecked == true); }
        private void CBFreePlanting_Click(object sender, RoutedEventArgs e) { PVZ.FreePlantingCheat = CBFreePlanting.IsChecked == true; }
        private void CBShowHidden_Click(object sender, RoutedEventArgs e) { PVZ.ShowHiddenLevel(CBShowHidden.IsChecked == true); }
        private void CBOverlapPlanting_Click(object sender, RoutedEventArgs e) { PVZ.OverlapPlanting(CBOverlapPlanting.IsChecked == true); }
        private void CBIgnoreRes_Click(object sender, RoutedEventArgs e) { PVZ.IgnoreRes(CBIgnoreRes.IsChecked == true); }
        private void CBNoCD_Click(object sender, RoutedEventArgs e) { PVZ.NoCD(CBNoCD.IsChecked == true); }
        private void CBConveyorBeltNoDelay_Click(object sender, RoutedEventArgs e) { PVZ.ConveyorBeltNoDelay(CBConveyorBeltNoDelay.IsChecked == true); }
        private void CBFullScreenFog_Click(object sender, RoutedEventArgs e) { PVZ.FullScreenFog(CBFullScreenFog.IsChecked == true); }
        private void CBBlockZombie_Click(object sender, RoutedEventArgs e) { PVZ.BlockZombie(CBBlockZombie.IsChecked == true); }
        private void CBNoUpperLimit_Click(object sender, RoutedEventArgs e) { PVZ.NoUpperLimit(CBNoUpperLimit.IsChecked == true); }
        private void CBVasePerspect_Click(object sender, RoutedEventArgs e) { PVZ.VasePerspect(CBVasePerspect.IsChecked == true); }
        private void CBFogPerspect_Click(object sender, RoutedEventArgs e) { PVZ.FogPerspect(CBFogPerspect.IsChecked == true); }
        private void CBLockShovel_Click(object sender, RoutedEventArgs e) { PVZ.LockShovel(CBLockShovel.IsChecked == true); }
        private void CBAutoCollect_Click(object sender, RoutedEventArgs e) { PVZ.AutoCollect(CBAutoCollect.IsChecked == true); }

        private void BtnKillAllZombies_Click(object sender, RoutedEventArgs e)
        {
            foreach (var zombie in PVZ.AllZombies) zombie.State = 3;
        }

        private void BtnHypnotizeAllZombies_Click(object sender, RoutedEventArgs e)
        {
            foreach (var zombie in PVZ.AllZombies) zombie.Hypnotized = true;
        }

        private void BtnKillAllPlants_Click(object sender, RoutedEventArgs e)
        {
            foreach (var plant in PVZ.AllPlants) plant.Exist = false;
        }

        private void BtnMonitor_Click(object sender, RoutedEventArgs e)
        {
            BtnMonitor.IsEnabled = false;
            new MonitorWindow().Show();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            BtnModify.IsEnabled = false;
            new ModifyWindow().Show();
        }

        private void BtnOperate_Click(object sender, RoutedEventArgs e)
        {
            BtnOperate.IsEnabled = false;
            new OperationWindow().Show();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                if (Lang.Id == 1)
                    MessageBox.Show(PVZ.LastWarning, "You got a warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show(PVZ.LastWarning, "你得到如下警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (e.Key == Key.F5) OpenGame();
            else if (e.Key == Key.F6) OpenMuiti();
        }

        private void OpenMuiti()
        {
            if (File.Exists(PVZ.GamePath))
            {
                try
                {
                    string temp = Path.Combine(Environment.GetEnvironmentVariable("Temp"), $"PlantsVsZombies_Temp{new Random().NextDouble() * 1000 * new Random().NextDouble() * 1000 * new Random().NextDouble()}.exe");
                    File.Copy(PVZ.GamePath, temp);
                    using (var f = File.OpenWrite(temp))
                    {
                        f.Seek(0x153F1B, SeekOrigin.Begin);
                        f.WriteByte(0xEB);
                    }
                    var pro = new Process();
                    pro.StartInfo = new ProcessStartInfo
                    {
                        FileName = temp,
                        WorkingDirectory = Path.GetDirectoryName(PVZ.GamePath)
                    };
                    pro.Start();
                }
                catch { }
            }
        }

        private void OpenGame()
        {
            try
            {
                if (PVZ.Game != null && PVZ.Game.HasExited)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = PVZ.GamePath,
                        WorkingDirectory = Path.GetDirectoryName(PVZ.GamePath)
                    });
                    FindGame();
                }
            }
            catch { }
        }

        private void expanderPlugIns_Expanded(object sender, RoutedEventArgs e)
        {
            BtnMonitor.Visibility = Visibility.Collapsed;
            BtnModify.Visibility = Visibility.Collapsed;
            BtnOperate.Visibility = Visibility.Collapsed;
        }

        private void expanderPlugIns_Collapsed(object sender, RoutedEventArgs e)
        {
            BtnMonitor.Visibility = Visibility.Visible;
            BtnModify.Visibility = Visibility.Visible;
            BtnOperate.Visibility = Visibility.Visible;
        }

        string[] StatusTextFound = { "已找到游戏", "Game Found" };
        string[] StatusTextNotSuppost = { "不支持的版本", "NotSuppost" };
        string[] StatusTextOpenFailed = { "打开游戏失败", "OpenFailed" };
        string[] StatusTextNotFound = { "没有找到游戏", "NotFound" };
        private OpenFileDialog openFileDlg;

        private void BtnFindGame_Click(object sender, RoutedEventArgs e) { FindGame(); }

        private void BtnFindGame_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var processSelector = new ProcessSelector();
                if (processSelector.ShowDialog() == true)
                {
                    if (PVZ.RunGame(processSelector.ProcessId))
                    {
                        var check = PVZ.CheckPeocess();
                        if (check.HasValue)
                        {
                            if (check.Value)
                            {
                                TBStatus.Text = StatusTextFound[Lang.Id];
                                PVZ.InitFunctions();
                                PVZ.Game.EnableRaisingEvents = true;
                                PVZ.Game.Exited += GameExited;
                            }
                            else TBStatus.Text = StatusTextNotSuppost[Lang.Id];
                        }
                        else TBStatus.Text = StatusTextOpenFailed[Lang.Id];
                    }
                    else TBStatus.Text = StatusTextNotFound[Lang.Id];
                }
            }
        }

        private void textBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
                {
                    Lang.Id += 1;
                    Lang.Id = Lang.Id % Lang.Count;
                    BtnFindGame_Click(null, null);
                    foreach (Window win in Application.Current.Windows)
                        Lang.ChangeLanguage(win.Content);
                }
                else if (e.RightButton == MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Released)
                {
                    var output = new ExpendWindow();
                    output.TBTitle.Text = Lang.Id == 1 ? "About" : "关于";
                    var text = new TextBox
                    {
                        Width = 578, Height = 295,
                        Background = Brushes.Transparent,
                        Foreground = Brushes.White,
                        IsReadOnly = true,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 16
                    };
                    Canvas.SetTop(text, 56);
                    Canvas.SetLeft(text, 10);
                    if (Lang.Id == 1)
                        text.Text = "This procedure is made by 冥谷川恋(email: lazuplismei@163.com )\nIt can be used to modify various contents of Plants vs.zombies,it's a modifier which provide powerful functions of monitoring, modification and operation\nYou can even manually plug it in to extend its capabilities(Just implement the interface in ITrainerExtension)\nAttentions:\nThe program is always free and can be used at will.\nIt only supposted 1.0.0.1051 version of Plants vs.zombies.\nFor later versions of 1.2.0.1063,1.2.0.1073 and the version from steam all invalid.";
                    else
                        text.Text = "本程序由冥谷川恋制作（QQ398833450，邮箱lazuplismei@163.com）\n可用于修改植物大战僵尸的各项内容，是一个提供监视，修改，操作等强大功能的修改器\n甚至可以手动为其编写插件来扩展它的功能（实现ITrainerExtension中的接口即可）\n注意事项：\n程序完全免费可任意使用\n程序仅对1.0.0.1051版本的植物大战僵尸有效\n对于更高版本的1.2.0.1063，1.2.0.1073以及来源于Steam上的版本均无效";
                    output.MainCanvas.Children.Add(text);
                    var btn = new DarkStyle.DarkButton
                    {
                        BorderThickness = new Thickness(1),
                        Width = 200,
                        Content = Lang.Id == 1 ? "Close" : "关闭",
                        FontSize = 20
                    };
                    Canvas.SetBottom(btn, 10);
                    Canvas.SetLeft(btn, 195);
                    btn.Click += output.Close;
                    output.MainCanvas.Children.Add(btn);
                    output.ShowDialog();
                }
            }
        }

        private void BtnFindGame_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed) OpenMuiti();
            else if (e.RightButton == MouseButtonState.Pressed) OpenGame();
        }
    }
}
