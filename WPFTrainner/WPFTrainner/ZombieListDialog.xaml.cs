using System;
using System.Windows;
using System.Windows.Input;
using ITrainerExtension;
using PVZClass;

namespace PVZWPFTrainner
{
    public partial class ZombieListDialog : Window
    {
        private PVZ.ZombieList.Wave currentWave;

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (System.InvalidOperationException) { }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((ModifyWindow)Owner).BtnZombieList.IsEnabled = true;
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LBZombieSeed.Items.Clear();
            foreach (var zseed in PVZ.ZombieSeed)
            {
                if (Lang.Id != 0)
                    LBZombieSeed.Items.Add(zseed.ToString());
                else
                    LBZombieSeed.Items.Add(zseed.GetDescription());
            }
            NudWaveNum.MaxValue = (int)PVZ.WaveNum;
            Lang.ChangeLanguage(Content);
        }

        private void NudWaveNum_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                currentWave = PVZ.ZombieList.GetWave((int)NudWaveNum.Value);
                FlushListItem();
            }
        }

        private void LBZombieLst_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsLoaded && LBZombieLst.SelectedIndex >= 0)
            {
                CBZombieTypes.IsReadOnly = true;
                CBZombieTypes.SelectedIndex = (int)((System.Windows.Controls.ListBoxItem)LBZombieLst.SelectedItem).Tag;
                CBZombieTypes.IsReadOnly = false;
            }
        }

        private void CBZombieTypes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsLoaded && CBZombieTypes.IsReadOnly != true && LBZombieLst.SelectedIndex >= 0)
            {
                var type = (PVZ.ZombieType)CBZombieTypes.SelectedIndex;
                if (currentWave != null) PVZ.Memory.WriteInteger(currentWave.BaseAddress + LBZombieLst.SelectedIndex * 4, (int)type);
                FlushListItem();
            }
        }

        private void AddZombie_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CBZombieTypesAdd.SelectedIndex >= 0)
            {
                var type = (PVZ.ZombieType)CBZombieTypesAdd.SelectedIndex;
                currentWave?.Add(type);
                FlushListItem();
            }
        }

        private void DelSelZombie_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LBZombieLst.SelectedIndex >= 0)
            {
                var type = (PVZ.ZombieType)CBZombieTypes.SelectedIndex;
                currentWave?.Del(LBZombieLst.SelectedIndex);
                FlushListItem();
            }
        }

        private void ClearZombies_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (currentWave != null) PVZ.Memory.WriteInteger(currentWave.BaseAddress, -1);
            FlushListItem();
        }

        private void FlushListItem()
        {
            LBZombieLst.Items.Clear();
            if (currentWave == null) return;
            foreach (var zombie in currentWave.All)
            {
                var item = new System.Windows.Controls.ListBoxItem();
                if (Lang.Id != 0)
                    item.Content = zombie.ToString();
                else
                    item.Content = zombie.GetDescription();
                item.Tag = zombie;
                LBZombieLst.Items.Add(item);
            }
        }

        private void FlushData_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window_Loaded(null, null);
            currentWave = PVZ.ZombieList.GetWave((int)NudWaveNum.Value);
            FlushListItem();
        }

        private void LBZombieLst_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
                DelSelZombie_Click(null, null);
            else if (e.Key == System.Windows.Input.Key.A && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
                AddZombie_Click(null, null);
        }

        public int scale = 100;

        private void Window_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (e.Delta > 0) scale += 5; else scale -= 5;
                scale = Math.Max(10, scale);
                scale = Math.Min(300, scale);
                System.Windows.UIElement con = Content as System.Windows.UIElement;
                con.RenderTransform = new System.Windows.Media.ScaleTransform(scale / 100.0, scale / 100.0);
                Height = 400.0 * scale / 100;
                Width = 420.0 * scale / 100;
            }
        }
    }

    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            return PVZClass.ExtensionModule.GetDescription(value);
        }
    }
}
