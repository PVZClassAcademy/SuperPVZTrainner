using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ITrainerExtension;

namespace PVZWPFTrainner
{
    public partial class ProcessSelector : Window
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string WindowName { get; set; }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (System.InvalidOperationException) { }
        }

        readonly DataTable dt = new DataTable();

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dt.Columns.Add("ProcessId");
            dt.Columns.Add("ProcessName");
            dt.Columns.Add("WindowName");
            foreach (var pro in Process.GetProcesses())
                dt.Rows.Add(pro.Id, pro.ProcessName, pro.MainWindowTitle);
            LVMain.DataContext = dt;
            if (Lang.Id == 1)
            {
                TBTitle.Text = "ProcessSelector(Click the header to sort)";
                GVCPID.Header = "PID";
                GVCName.Header = "Name";
                GVCTitle.Header = "Window Title";
                BtnSelect.Content = "Select";
                BtnRefresh.Content = "Refresh";
                BtnCancel.Content = "Cancel";
            }
            else
            {
                TBTitle.Text = "进程选择窗口(点击表头可排序)";
                GVCPID.Header = "进程ID";
                GVCName.Header = "进程名";
                GVCTitle.Header = "窗口标题";
                BtnSelect.Content = "选择";
                BtnRefresh.Content = "刷新";
                BtnCancel.Content = "取消";
            }
        }

        private void ListViewSort(System.Windows.Controls.ListView lv, string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void LVMain_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var gch = e.OriginalSource as System.Windows.Controls.GridViewColumnHeader;
            ListSortDirection sort;
            if (gch.Tag != null && (bool)gch.Tag)
            {
                gch.Tag = false;
                sort = ListSortDirection.Ascending;
            }
            else
            {
                gch.Tag = true;
                sort = ListSortDirection.Descending;
            }
            if (gch.Content?.ToString() == "进程ID" || gch.Content?.ToString() == "PID")
                ListViewSort(LVMain, "ProcessId", sort);
            else if (gch.Content?.ToString() == "进程名" || gch.Content?.ToString() == "Name")
                ListViewSort(LVMain, "ProcessName", sort);
            else if (gch.Content?.ToString() == "窗口标题" || gch.Content?.ToString() == "Window Title")
                ListViewSort(LVMain, "WindowName", sort);
        }

        private void BtnSelect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LVMain.SelectedIndex >= 0)
            {
                DialogResult = true;
                var row = ((DataRowView)LVMain.SelectedItem).Row;
                ProcessId = int.Parse(row[0].ToString());
                ProcessName = row[1].ToString();
                WindowName = row[2].ToString();
            }
        }

        private void BtnRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dt.Rows.Clear();
            foreach (var pro in Process.GetProcesses())
                dt.Rows.Add(pro.Id, pro.ProcessName, pro.MainWindowTitle);
            LVMain.DataContext = dt;
        }

        private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LVMain_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LVMain.SelectedIndex >= 0)
            {
                DialogResult = true;
                var row = ((DataRowView)LVMain.SelectedItem).Row;
                ProcessId = int.Parse(row[0].ToString());
                ProcessName = row[1].ToString();
                WindowName = row[2].ToString();
            }
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
                Height = 500.0 * scale / 100;
                Width = 600.0 * scale / 100;
            }
        }
    }
}
