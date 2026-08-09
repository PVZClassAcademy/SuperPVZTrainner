using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ITrainerExtension;
using PVZClass;

namespace PVZWPFTrainner
{
    public partial class WaveManageDialog : Window
    {
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (System.InvalidOperationException) { }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((ModifyWindow)Owner).BtnWaveManager.IsEnabled = true;
        }

        private void NudAdvStage_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                int level = ((int)NudAdvStage.Value - 1) * 10 + (int)NudAdvLevel.Value - 1;
                NudAdvWave.IgnoreAssign = true;
                NudAdvWave.Value = (int)PVZ.Memory.ReadInteger(0x6A34E8 + level * 4);
                NudAdvWave.IgnoreAssign = false;
            }
        }

        private void NudAdvWave_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                int level = ((int)NudAdvStage.Value - 1) * 10 + (int)NudAdvLevel.Value - 1;
                PVZ.Memory.WriteInteger(0x6A34E8 + level * 4, (int)(int)NudAdvWave.Value);
            }
        }

        private int[] WavesList = new int[] { 0x4092FD, 0x40932C, 0x4093F2, 0x409466, 0x409472, 0x40947E, 0x40948A, 0x409499 };
        private List<int> TypesList = new List<int>(new int[] { 0x409394, 0x409326, 0x4093EC, 0x409460, 0x40946C, 0x409478, 0x409489, 0x409498 });

        private void CBWaveTypes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                NudTypesWave.IgnoreAssign = true;
                NudTypesWave.Value = (int)PVZ.Memory.ReadInteger(WavesList[CBWaveTypes.SelectedIndex]);
                NudTypesWave.IgnoreAssign = false;
            }
        }

        private void NudTypesWave_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
                PVZ.Memory.WriteInteger(WavesList[CBWaveTypes.SelectedIndex], (int)(int)NudTypesWave.Value);
        }

        private void NudAdvWaveCmp_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
                PVZ.Memory.WriteByte(0x409391, (byte)(int)NudAdvWaveCmp.Value);
        }

        private void NudAdvWaveAdd_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
                PVZ.Memory.WriteByte(0x4093A1, (byte)(int)NudAdvWaveAdd.Value);
        }

        private int[] LevelsList = new int[] { 0x4093C0, 0x4093FE, 0x409403, 0x409408, 0x40940D, 0x409412,
            0x409417, 0x409420, 0x40942D, 0x409436, 0x40943F, 0x409444, 0x40944F, 0x409454, 0x409459, 0x40945E };

        private void CBLevels_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                int des = PVZ.GetJumpDestination(LevelsList[CBLevels.SelectedIndex]);
                CBLevelTypes.Tag = true;
                CBLevelTypes.SelectedIndex = TypesList.IndexOf(des);
                CBLevelTypes.Tag = false;
            }
        }

        private void CBLevelTypes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsLoaded && CBLevelTypes.Tag != null && !(bool)CBLevelTypes.Tag)
            {
                if (!PVZ.SetJumpDestination(LevelsList[CBLevels.SelectedIndex], TypesList[CBLevelTypes.SelectedIndex]))
                {
                    if (Lang.Id == 1)
                        System.Windows.MessageBox.Show("code distance too far,short jump does not support this modification", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    else
                        System.Windows.MessageBox.Show("指令距离太远,短程jmp不支持此项修改", "信息", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
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
                Height = 215.0 * scale / 100;
                Width = 400.0 * scale / 100;
            }
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Lang.ChangeLanguage(Content);
        }
    }
}
