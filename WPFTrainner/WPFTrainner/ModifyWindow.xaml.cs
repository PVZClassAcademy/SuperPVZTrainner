using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ITrainerExtension;
using PVZClass;

namespace PVZWPFTrainner
{
    public partial class ModifyWindow : Window
    {
        private bool DealKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) { ((TextBox)sender).Text = "0"; e.Handled = true; }
            else if (e.Key == Key.V) { e.Handled = true; }
            return e.Key == Key.Enter;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (InvalidOperationException) { }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow != null)
                ((MainWindow)Application.Current.MainWindow).BtnModify.IsEnabled = true;
        }

        private void NudAdvStage_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                if (NudAdvStage.Value == 6) NudAdvLevel.MaxValue = int.MaxValue - 50;
                else { NudAdvLevel.Value = Math.Min((int)NudAdvLevel.Value, 10); NudAdvLevel.MaxValue = 10; }
            }
        }

        string[] jlText1 = { "非常抱歉,冒险模式1-1不能在关内跳关", "sorry, Adventure mode 1-1 can't jump level while playing" };
        string[] jlText2 = { "跳关已设置,请点击游戏中右上角的菜单按钮", "Function set,Please click the Main menu button in the upper right corner of the game" };

        private void BtnJumpLevel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int advLevel = 0; byte gameLevel = 0;
            GetLevel(ref advLevel, ref gameLevel);
            int msg = PVZ.JumpLevel(advLevel, gameLevel);
            if (msg == 0) MessageBox.Show(jlText1[Lang.Id], "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (msg == 1) MessageBox.Show(jlText2[Lang.Id], "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GetLevel(ref int advLevel, ref byte gameLevel)
        {
            if (RBAdventure.IsChecked != null && RBAdventure.IsChecked.Value) advLevel = 10 * ((int)NudAdvStage.Value - 1) + (int)(int)NudAdvLevel.Value;
            else if (RBSurvival.IsChecked != null && RBSurvival.IsChecked.Value) { advLevel = PVZ.SaveData.AdventureLevel; gameLevel = (byte)(CBLevelSurvival.SelectedIndex + 1); }
            else if (RBMiniGames.IsChecked != null && RBMiniGames.IsChecked.Value) { advLevel = PVZ.SaveData.AdventureLevel; gameLevel = (byte)(CBLevelMini.SelectedIndex + 16); }
            else if (RBMiniHidden.IsChecked != null && RBMiniHidden.IsChecked.Value)
            {
                advLevel = PVZ.SaveData.AdventureLevel;
                gameLevel = CBLevelMiniHidden.SelectedIndex >= 15 ? (byte)(CBLevelMiniHidden.SelectedIndex + 56) : (byte)(CBLevelMiniHidden.SelectedIndex + 36);
            }
            else if (RBPuzzle.IsChecked != null && RBPuzzle.IsChecked.Value)
            {
                advLevel = PVZ.SaveData.AdventureLevel;
                gameLevel = CBLevelPuzzle.SelectedIndex >= 9 ? (byte)(CBLevelPuzzle.SelectedIndex + 52) : (byte)(CBLevelPuzzle.SelectedIndex + 51);
            }
            else if (RBEndless.IsChecked != null && RBEndless.IsChecked.Value)
            {
                advLevel = PVZ.SaveData.AdventureLevel;
                if (CBLevelEndless.SelectedIndex == 5) gameLevel = 60;
                else if (CBLevelEndless.SelectedIndex == 6) gameLevel = 70;
                else gameLevel = (byte)(CBLevelEndless.SelectedIndex + 11);
            }
        }

        private void BtnToLevel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int advLevel = 0; byte gameLevel = 0;
            GetLevel(ref advLevel, ref gameLevel);
            PVZ.AdventureLevel = advLevel;
            PVZ.SaveData.AdventureLevel = advLevel;
            PVZ.LevelId = (PVZ.Level)gameLevel;
        }

        private void BtnGoldenSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PVZ.SaveData.AdventureFinishCount = Math.Max(1, PVZ.SaveData.AdventureFinishCount);
            int addr = PVZ.SaveData.BaseAddress + 0x30;
            int finishedCount;
            for (int index = 0; index <= 4; index++) { finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index); PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 5)); }
            for (int index = 5; index <= 9; index++) { finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index); PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 10)); }
            addr = PVZ.SaveData.BaseAddress + 0x6C;
            for (int index = 0; index <= 34; index++) { finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index); PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 1)); }
            addr = PVZ.SaveData.BaseAddress + 0xF8;
            for (int index = 0; index <= 8; index++) { finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index); PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 1)); }
            addr = PVZ.SaveData.BaseAddress + 0x120;
            for (int index = 0; index <= 8; index++) { finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index); PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 1)); }
            finishedCount = PVZ.Memory.ReadInteger(PVZ.SaveData.BaseAddress + 0x148); PVZ.Memory.WriteInteger(PVZ.SaveData.BaseAddress + 0x148, Math.Max(finishedCount, 1));
            finishedCount = PVZ.Memory.ReadInteger(PVZ.SaveData.BaseAddress + 0x14C); PVZ.Memory.WriteInteger(PVZ.SaveData.BaseAddress + 0x14C, Math.Max(finishedCount, 1));
        }

        private void BtnWin_Click(object sender, System.Windows.RoutedEventArgs e) { if (PVZ.ExitingLevelCountDown == -1) PVZ.Win(); }

        private void DaveSelCardNum_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool flag = Lang.Id == 0;
            var inputDlg = new InputDialog(flag ? "请输入数值" : "Please enter a value", flag ? "选卡张数" : "Number of cards", 1, 10);
            if (inputDlg.ShowDialog() == true) PVZ.Memory.WriteByte(0x48420B, (byte)inputDlg.Value);
        }

        private void BtnCreatePortal_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PVZ.CreatePortal((int)NvdBlack1Y.Value, (int)NvdBlack1X.Value, (int)NvdBlack2Y.Value, (int)NvdBlack2X.Value, (int)NvdBlue1Y.Value, (int)NvdBlue1X.Value, (int)NvdBlue2Y.Value, (int)NvdBlue2X.Value);
        }

        private void GraveAppearWave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool flag = Lang.Id == 0;
            var inputDlg = new InputDialog(flag ? "请输入数值" : "Please enter a value", flag ? "墓碑出现的波数" : "Grave appear at wave", 3, byte.MaxValue);
            if (inputDlg.ShowDialog() == true) PVZ.Memory.WriteByte(0x426925, (byte)(inputDlg.Value - 2));
        }

        private void CBLockIZEFormat_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CBLockIZEFormat.IsChecked == true)
            {
                CBIZEFormat.IsEnabled = false;
                byte[][] data = {
                    new byte[]{0x0F,0x81,0x68,0x01,0x00,0x00}, new byte[]{0x0F,0x81,0xF9,0x00,0x00,0x00},
                    new byte[]{0x0F,0x81,0xBE,0x00,0x00,0x00}, new byte[]{0x0F,0x81,0x18,0x00,0x00,0x00},
                    new byte[]{0x0F,0x81,0x90,0xFD,0xFF,0xFF}, new byte[]{0x0F,0x81,0x49,0x00,0x00,0x00},
                    new byte[]{0x0F,0x81,0x64,0x00,0x00,0x00}, new byte[]{0x0F,0x81,0x7A,0x00,0x00,0x00}
                };
                if (CBIZEFormat.SelectedIndex >= 0 && CBIZEFormat.SelectedIndex < data.Length)
                    PVZ.Memory.WriteByteArray(0x42B046, data[CBIZEFormat.SelectedIndex]);
            }
            else
            {
                CBIZEFormat.IsEnabled = true;
                PVZ.Memory.WriteByteArray(0x42B046, new byte[] { 0x0F, 0x85, 0x90, 0x00, 0x00, 0x00 });
            }
        }

        private void CBPlantStaticProp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                if (CBPlantStaticProp.SelectedIndex == 0)
                    TBPlantStaicProp.Text = PVZ.Memory.ReadInteger(0x69F2B8 + CBPlantTypes.SelectedIndex * 0x24).ToString();
                else
                    TBPlantStaicProp.Text = PVZ.Memory.ReadInteger(0x69F2BC + CBPlantStaticProp.SelectedIndex * 4 + CBPlantTypes.SelectedIndex * 0x24).ToString();
            }
        }

        private void TBPlantStaicProp_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DealKeyDown(sender, e))
            {
                if (CBPlantStaticProp.SelectedIndex == 0)
                    PVZ.Memory.WriteInteger(0x69F2B8 + CBPlantTypes.SelectedIndex * 0x24, Convert.ToInt32(TBPlantStaicProp.Text));
                else
                    PVZ.Memory.WriteInteger(0x69F2BC + CBPlantStaticProp.SelectedIndex * 4 + CBPlantTypes.SelectedIndex * 0x24, Convert.ToInt32(TBPlantStaicProp.Text));
            }
        }

        private int[] plantTimePropAddrs = { 0x45E300, 0x45E34E, 0x4613BC, 0x461551, 0x45E3F1, 0x45FCE3, 0x4632B0, 0x460DFE, 0x460A3D, 0x460AF1, 0x460B56, 0x460C53, 0x460D21, 0x4600F1, 0x45DF05, 0x46163A, 0x45E521, 0x45E560, 0x464D4D };

        private void CBPlantTimeProp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && CBPlantTimeProp.SelectedIndex >= 0 && CBPlantTimeProp.SelectedIndex < plantTimePropAddrs.Length)
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(plantTimePropAddrs[CBPlantTimeProp.SelectedIndex]).ToString();
        }

        private void TBPlantTimeProp_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DealKeyDown(sender, e) && CBPlantTimeProp.SelectedIndex >= 0 && CBPlantTimeProp.SelectedIndex < plantTimePropAddrs.Length)
            {
                int val = Convert.ToInt32(TBPlantTimeProp.Text);
                PVZ.Memory.WriteInteger(plantTimePropAddrs[CBPlantTimeProp.SelectedIndex], val);
                if (CBPlantTimeProp.SelectedIndex == 6 || CBPlantTimeProp.SelectedIndex == 7)
                {
                    int mt = PVZ.Memory.ReadInteger(0x45DC5F);
                    PVZ.Memory.WriteInteger(0x45DC5F, Math.Max(mt, val));
                }
            }
        }

        private int[] plantHpAddrs = { 0x45DC55, 0x45E1A7, 0x45E215, 0x45E445, 0x45E242, 0x45E5C3 };

        private void CBPlantHp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && CBPlantHp.SelectedIndex >= 0 && CBPlantHp.SelectedIndex < plantHpAddrs.Length)
                TBPlantHp.Text = PVZ.Memory.ReadInteger(plantHpAddrs[CBPlantHp.SelectedIndex]).ToString();
        }

        private void TBPlantHp_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DealKeyDown(sender, e) && CBPlantHp.SelectedIndex >= 0)
                PVZ.Memory.WriteInteger(plantHpAddrs[CBPlantHp.SelectedIndex], Convert.ToInt32(TBPlantHp.Text));
        }

        private void CBPlantInvincible_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PlantInvincibleResume();
            if (CBPlantInvincible.IsChecked == true)
            {
                CBPlantWeak.IsEnabled = false;
                if (MIPlantBiteProof.IsChecked == true) PVZ.Memory.WriteByte(0x52FCF3, 0);
                if (MIPlantBlastProof.IsChecked == true) PVZ.Memory.WriteByte(0x41CC2F, 235);
                if (MIPlantRollProof.IsChecked == true) { PVZ.Memory.WriteByte(0x45EC66, 0); PVZ.Memory.WriteByte(0x45EE0A, 112); PVZ.Memory.WriteByte(0x52E93B, 235); PVZ.Memory.WriteInteger(0x462B80, 1811940546); }
                if (MIPlantHitProof.IsChecked == true) { PVZ.Memory.WriteInteger(0x46CFEB, -2087677808); PVZ.Memory.WriteInteger(0x46D7A6, -2087677808); }
                if (MIPlantBurnProof.IsChecked == true) PVZ.Memory.WriteByte(0x5276EA, 235);
            }
            else CBPlantWeak.IsEnabled = true;
        }

        private static void PlantInvincibleResume()
        {
            PVZ.Memory.WriteByte(0x41CC2F, 116); PVZ.Memory.WriteByte(0x45EC66, 224); PVZ.Memory.WriteByte(0x45EE0A, 117);
            PVZ.Memory.WriteInteger(0x46CFEB, -2092937175); PVZ.Memory.WriteInteger(0x46D7A6, -2092937687);
            PVZ.Memory.WriteByte(0x5276EA, 117); PVZ.Memory.WriteByte(0x52E93B, 116); PVZ.Memory.WriteByte(0x52FCF3, 252);
            PVZ.Memory.WriteByte(0x52FCF1, 70); PVZ.Memory.WriteInteger(0x462B80, 1821070675);
        }

        private void CBPlantWeak_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PlantInvincibleResume();
            if (CBPlantWeak.IsChecked == true) { CBPlantInvincible.IsEnabled = false; PVZ.Memory.WriteByte(0x45EE0A, 112); PVZ.Memory.WriteByte(0x46CFEC, 64); PVZ.Memory.WriteByte(0x46D7A7, 118); PVZ.Memory.WriteByte(0x52FCF3, 0); PVZ.Memory.WriteByte(0x52FCF1, 102); }
            else CBPlantInvincible.IsEnabled = true;
        }

        private void CBZombieStaticProp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            int[] addrs = { 0x69DA84, 0x69DA88, 0x69DA94 };
            if (CBZombieStaticProp.SelectedIndex >= 0 && CBZombieStaticProp.SelectedIndex < 3)
                TBZombieStaicProp.Text = PVZ.Memory.ReadInteger(addrs[CBZombieStaticProp.SelectedIndex] + CBZombieTypes.SelectedIndex * 0x1C).ToString();
        }

        private void TBZombieStaicProp_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DealKeyDown(sender, e))
            {
                int[] addrs = { 0x69DA84, 0x69DA88, 0x69DA94 };
                if (CBZombieStaticProp.SelectedIndex >= 0 && CBZombieStaticProp.SelectedIndex < 3)
                    PVZ.Memory.WriteInteger(addrs[CBZombieStaticProp.SelectedIndex] + CBZombieTypes.SelectedIndex * 0x1C, Convert.ToInt32(TBZombieStaicProp.Text));
            }
        }

        private void CBZombieCardSun_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (CBZombieCardSun.SelectedIndex < 7) TBZombieCardSun.Text = PVZ.Memory.ReadInteger(0x467B60 + CBZombieCardSun.SelectedIndex * 6).ToString();
            else if (CBZombieCardSun.SelectedIndex == 7) TBZombieCardSun.Text = PVZ.Memory.ReadInteger(0x467B3D).ToString();
            else if (CBZombieCardSun.SelectedIndex == 8) TBZombieCardSun.Text = PVZ.Memory.ReadInteger(0x467B48).ToString();
        }

        private void TBZombieCardSun_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!DealKeyDown(sender, e)) return;
            int val = Convert.ToInt32(TBZombieCardSun.Text);
            if (CBZombieCardSun.SelectedIndex < 7) PVZ.Memory.WriteInteger(0x467B60 + CBZombieCardSun.SelectedIndex * 6, val);
            else if (CBZombieCardSun.SelectedIndex == 7) PVZ.Memory.WriteInteger(0x467B3D, val);
            else if (CBZombieCardSun.SelectedIndex == 8) PVZ.Memory.WriteInteger(0x467B48, val);
        }

        private void CBZombieCardId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (PVZ.Memory.ReadInteger(0x42A046) == -125582400) { PVZ.Memory.WriteQword(0x42A03E, 202333581277315); PVZ.Memory.WriteShort(0x42A046, 0); PVZ.Memory.WriteByte(0x4661BE, 124); PVZ.Memory.WriteByte(0x42A41A, 32); }
            if (CBZombieCardId.SelectedIndex == 0) CBZombieTypes2.SelectedIndex = PVZ.Memory.ReadInteger(0x42A044);
            else CBZombieTypes2.SelectedIndex = PVZ.Memory.ReadInteger(0x42A04E + (CBZombieCardId.SelectedIndex - 1) * 11);
        }

        private void CBZombieTypes2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CBZombieTypes2.IsMouseOver) return;
            if (PVZ.Memory.ReadInteger(0x42A046) == -125582400) { PVZ.Memory.WriteQword(0x42A03E, 202333581277315); PVZ.Memory.WriteShort(0x42A046, 0); PVZ.Memory.WriteByte(0x4661BE, 124); PVZ.Memory.WriteByte(0x42A41A, 32); }
            if (CBZombieCardId.SelectedIndex == 0) PVZ.Memory.WriteInteger(0x42A044, CBZombieTypes2.SelectedIndex);
            else PVZ.Memory.WriteInteger(0x42A04E + (CBZombieCardId.SelectedIndex - 1) * 11, CBZombieTypes2.SelectedIndex);
        }

        private int[] zombieTimePropAddrs = { 0x52350A, 0x528EB7, 0x523160, 0x522FBD, 0x522FDB, 0x522FE0, 0x528355, 0x5232A7, 0x525548, 0x522978, 0x525127, 0x525A28, 0x525B28, 0x523BD0, 0x5275B2, 0x523A7A, 0x523A91, 0x523BD0, 0x527831, 0x527BAE, 0x527D20, 0x527E4A };

        private void CBZombieTimeProp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && CBZombieTimeProp.SelectedIndex >= 0 && CBZombieTimeProp.SelectedIndex < zombieTimePropAddrs.Length)
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(zombieTimePropAddrs[CBZombieTimeProp.SelectedIndex]).ToString();
        }

        private void TBZombieTimeProp_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DealKeyDown(sender, e) && CBZombieTimeProp.SelectedIndex >= 0 && CBZombieTimeProp.SelectedIndex < zombieTimePropAddrs.Length)
                PVZ.Memory.WriteInteger(zombieTimePropAddrs[CBZombieTimeProp.SelectedIndex], Convert.ToInt32(TBZombieTimeProp.Text));
        }

        private int[] ZombieHpList = { 0x5227BB, 0x522892, 0x522CBF, 0x52292B, 0x52337D, 0x522949, 0x522BB0, 0x523530, 0x522DE1, 0x523139, 0x522D64, 0x522FC7, 0x522BEF, 0x523300, 0x52296E, 0x522A1B, 0x52299C, 0x522E8D, 0x523D26, 0x523624, 0x52361E, 0x52382B, 0x523A87, 0x52395D, 0x523E4A, 0x5235AC, 0x5234BF };

        private void CBZombieHp_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (IsLoaded) TBZombieHp.Text = PVZ.Memory.ReadInteger(ZombieHpList[CBZombieHp.SelectedIndex]).ToString(); }
        private void TBZombieHp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(ZombieHpList[CBZombieHp.SelectedIndex], Convert.ToInt32(TBZombieHp.Text)); }

        private void CBZombieInvincible_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ZombieInvincibleResume();
            if (CBZombieInvincible.IsChecked == true)
            {
                CBZombieWeak.IsEnabled = false;
                if (MIZombieChomperProof.IsChecked == true) PVZ.Memory.WriteByte(0x46144A, 235);
                if (MIZombieBlastEffectProof.IsChecked == true) PVZ.Memory.WriteByte(0x532BA1, 129);
                if (MIZombieBlastProof.IsChecked == true) { PVZ.Memory.WriteByte(0x41D8FF, 235); PVZ.Memory.WriteByte(0x4664F2, 235); }
                if (MIZombieSputterProof.IsChecked == true) PVZ.Memory.WriteByte(0x46D455, 235);
                if (MIZombieBloverProof.IsChecked == true) PVZ.Memory.WriteInteger(0x466601, -125595504);
                if (MIZombieHypnoshroonProof.IsChecked == true) PVZ.Memory.WriteByte(0x52FA82, 0);
                if (MIZombieLawnmoverProof.IsChecked == true) PVZ.Memory.WriteByte(0x458836, 235);
                if (MIZombieBodyDamageProof.IsChecked == true) PVZ.Memory.WriteInteger(0x53130F, -1869574000);
                if (MIZombieType1DamageProof.IsChecked == true) PVZ.Memory.WriteByte(0x531045, 192);
                if (MIZombieType2DamageProof.IsChecked == true) PVZ.Memory.WriteInteger(0x530C9B, -768360397);
            }
            else CBZombieWeak.IsEnabled = true;
        }

        private void ZombieInvincibleResume()
        {
            PVZ.Memory.WriteByte(0x46144A, 116); PVZ.Memory.WriteByte(0x532BA1, 141); PVZ.Memory.WriteByte(0x41D8FF, 127); PVZ.Memory.WriteByte(0x4664F2, 117);
            PVZ.Memory.WriteByte(0x46D455, 116); PVZ.Memory.WriteInteger(0x466601, -125631116); PVZ.Memory.WriteByte(0x52FA82, 1); PVZ.Memory.WriteByte(0x458836, 116);
            PVZ.Memory.WriteInteger(0x53130F, 539261995); PVZ.Memory.WriteByte(0x531045, 200); PVZ.Memory.WriteInteger(0x530C9B, -1031077252);
        }

        private void CBZombieWeak_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ZombieInvincibleResume();
            if (CBZombieWeak.IsChecked == true) { CBZombieInvincible.IsEnabled = false; PVZ.Memory.WriteShort(0x530C9B, -12149); PVZ.Memory.WriteByte(0x531045, 201); PVZ.Memory.WriteInteger(0x53130F, -1869545685); }
            else CBZombieInvincible.IsEnabled = true;
        }

        private void NudAdvStage2_ValueChanged(object sender, EventArgs e)
        {
            if (!IsLoaded) return;
            int level = 10 * ((int)NudAdvStage2.Value - 1) + (int)NudAdvLevel2.Value - 1;
            for (int index = 0; index <= 32; index++)
                ((CheckBox)CZombieSeeds.Children[index]).IsChecked = Convert.ToBoolean(PVZ.Memory.ReadInteger(0x6A35B4 + level * 4 + index * 0xCC));
        }

        private void BtnSetZombieSeed_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (RBAdventure2.IsChecked == true)
            {
                PVZ.Memory.WriteByte(0x40D6A3, 235);
                int level = 10 * ((int)NudAdvStage2.Value - 1) + (int)NudAdvLevel2.Value - 1;
                for (int index = 0; index <= 32; index++)
                    PVZ.Memory.WriteInteger(0x6A35B4 + level * 4 + index * 0xCC, Convert.ToInt32(((CheckBox)CZombieSeeds.Children[index]).IsChecked));
            }
            else if (RBMini2.IsChecked == true)
            {
                int count = 0;
                for (int index = 0; index <= 32; index++)
                {
                    var cbox = (CheckBox)CZombieSeeds.Children[index];
                    if (cbox.IsChecked == true) { PVZ.Memory.WriteInteger(ZombieSeedList[CBLevelMiniAll.SelectedIndex] + 9 * count, 0x54D4 + index); count++; }
                }
            }
            else if (RBCurrent.IsChecked == true)
            {
                int waveNum = (int)NudAdvFlags.Value;
                PVZ.Memory.WriteByteArray(0x409301, new byte[] { 0xEB, 0x23, 0x90, 0x90, 0x90, 0x90 });
                PVZ.Memory.WriteInteger(0x40932C, waveNum);
                for (int index = 0; index <= 32; index++) PVZ.Memory.WriteByte(PVZ.BaseAddress + 0x54D4 + index, 0);
                if (CBMaxLimit.IsChecked == true)
                {
                    int zcount = 0;
                    for (int index = 0; index <= 32; index++)
                    {
                        if (((CheckBox)CZombieSeeds.Children[index]).IsChecked == true) { zcount++; PVZ.Memory.WriteByte(PVZ.BaseAddress + 0x54D4 + index, 1); }
                    }
                    var random = new Random();
                    var zseeds = PVZ.ZombieSeed;
                    for (int index = 0; index <= waveNum - 1; index++)
                        for (int jndex = 0; jndex <= 49; jndex++)
                            PVZ.Memory.WriteInteger(PVZ.BaseAddress + 0x6B4 + jndex * 4 + index * 50 * 4, (int)zseeds[random.Next(zseeds.Length)]);
                    PVZ.ClearZombiePreview();
                    PVZ.ShowZombiePreview();
                }
                else
                {
                    var cbox0 = (CheckBox)CZombieSeeds.Children[0];
                    if (cbox0.IsChecked == true || ((CheckBox)CZombieSeeds.Children[26]).IsChecked == true)
                    {
                        for (int index = 0; index <= 32; index++)
                            if (((CheckBox)CZombieSeeds.Children[index]).IsChecked == true) PVZ.Memory.WriteByte(PVZ.BaseAddress + 0x54D4 + index, 1);
                        PVZ.CallZombieList();
                    }
                    else
                    {
                        int flag = -1;
                        for (int index = 0; index <= 32; index++)
                        {
                            var cbox = (CheckBox)CZombieSeeds.Children[index];
                            if (cbox.IsChecked == true && flag == -1) { flag = index; PVZ.Memory.WriteByte(PVZ.BaseAddress + 0x54D4 + 26, 1); }
                            else if (cbox.IsChecked == true) PVZ.Memory.WriteByte(PVZ.BaseAddress + 0x54D4 + index, 1);
                        }
                        PVZ.CallZombieList();
                        for (int index = 0; index <= waveNum - 1; index++)
                            for (int jndex = 0; jndex <= 49; jndex++)
                            {
                                int z = PVZ.Memory.ReadInteger(PVZ.BaseAddress + 0x6B4 + jndex * 4 + index * 50 * 4);
                                if (z == -1) break;
                                else if (z == 26) PVZ.Memory.WriteInteger(PVZ.BaseAddress + 0x6B4 + jndex * 4 + index * 50 * 4, flag);
                            }
                    }
                    PVZ.ClearZombiePreview();
                    PVZ.ShowZombiePreview();
                }
                PVZ.Memory.WriteByteArray(0x409301, new byte[] { 0x0F, 0x85, 0xA6, 0, 0, 0 });
                PVZ.Memory.WriteInteger(0x40932C, 8);
            }
        }

        private int[] ZombieSeedList = { 0x425C09, 0x425A6F, 0x42588F, 0x4258BD, 0x4258D4, 0x425902, 0x425942, 0x425974, 0x4259A2, 0x4259E2, 0x425A34, 0x425A94, 0x425AB0, 0x425CEC, 0x425AD5, 0x425B39, 0x425B67, 0x425BA7, 0x425BC3, 0x425CC6, 0x425C36, 0x425C83, 0x425CA9 };

        private void CBLevelMiniAll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            foreach (CheckBox cbox in CZombieSeeds.Children) cbox.IsChecked = false;
            int count = int.Parse(((ComboBoxItem)CBLevelMiniAll.SelectedItem).Tag.ToString());
            for (int index = 0; index < count; index++)
            {
                int i = PVZ.Memory.ReadInteger(ZombieSeedList[CBLevelMiniAll.SelectedIndex] + 9 * index) - 0x54D4;
                i = Math.Min(CZombieSeeds.Children.Count, Math.Max(0, i));
                ((CheckBox)CZombieSeeds.Children[i]).IsChecked = true;
            }
        }

        private void CBZombieSeed_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (RBMini2.IsChecked == true)
            {
                int checkedCount = 0;
                foreach (CheckBox cbox in CZombieSeeds.Children) if (cbox.IsChecked == true) checkedCount++;
                BtnSetZombieSeed.IsEnabled = checkedCount != int.Parse(((ComboBoxItem)CBLevelMiniAll.SelectedItem).Tag.ToString()) ? false : true;
            }
        }

        private void RBMini2_Checked(object sender, System.Windows.RoutedEventArgs e) { CBLevelMiniAll_SelectionChanged(null, null); }

        private void RBCurrent_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (CheckBox cbox in CZombieSeeds.Children) cbox.IsChecked = false;
            foreach (int zseed in PVZ.ZombieSeed) ((CheckBox)CZombieSeeds.Children[zseed]).IsChecked = true;
        }

        private void BtnWaveManager_Click(object sender, System.Windows.RoutedEventArgs e) { var waveDlg = new WaveManageDialog(); BtnWaveManager.IsEnabled = false; waveDlg.Owner = this; waveDlg.Show(); }
        private void BtnZombieList_Click(object sender, System.Windows.RoutedEventArgs e) { var zlistDlg = new ZombieListDialog(); BtnZombieList.IsEnabled = false; zlistDlg.Owner = this; zlistDlg.Show(); }

        private void TCMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                var selItem = (TabItem)TCMain.SelectedItem;
                selItem.Tag = TCMain;
                TCMain.Items.Remove(selItem);
                var separate = new SeparateWindow();
                separate.TCMain.Items.Add(selItem);
                separate.Show();
            }
        }

        private void RBDamagePlantType_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NudBound.IgnoreAssign = true; NudRadius.IgnoreAssign = true;
            if (RBDamageDoomShroom.IsChecked == true)
            {
                NudRadius.MaxValue = int.MaxValue;
                CBIsCinder.IsChecked = Convert.ToBoolean(PVZ.Memory.ReadByte(0x466835));
                NudBound.Value = PVZ.Memory.ReadByte(0x466837);
                NudRadius.Value = PVZ.Memory.ReadInteger(0x466839);
            }
            else
            {
                NudRadius.MaxValue = 127;
                int addrBase = 0;
                if (RBDamageExpWallNut.IsChecked == true) addrBase = 0x462E7A;
                else if (RBDamageCherryBomb.IsChecked == true) addrBase = 0x4667D7;
                else if (RBDamagePotatoMine.IsChecked == true) addrBase = 0x466A61;
                else if (RBDamageCobCannon.IsChecked == true) addrBase = 0x46D839;
                else if (RBDamageJackH.IsChecked == true) addrBase = 0x526C51;
                else if (RBDamageJack.IsChecked == true) addrBase = 0x526C6D;
                if (addrBase != 0) { CBIsCinder.IsChecked = Convert.ToBoolean(PVZ.Memory.ReadByte(addrBase)); NudBound.Value = PVZ.Memory.ReadByte(addrBase + 2); NudRadius.Value = (RBDamageExpWallNut.IsChecked == true) ? PVZ.Memory.ReadByte(addrBase + 4) : PVZ.Memory.ReadByte(addrBase + 4); }
            }
            NudBound.IgnoreAssign = false; NudRadius.IgnoreAssign = false;
        }

        private void CBIsCinder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            byte value = Convert.ToByte(CBIsCinder.IsChecked.Value);
            if (RBDamageExpWallNut.IsChecked == true) PVZ.Memory.WriteByte(0x462E7A, value);
            else if (RBDamageCherryBomb.IsChecked == true) PVZ.Memory.WriteByte(0x4667D7, value);
            else if (RBDamageDoomShroom.IsChecked == true) PVZ.Memory.WriteByte(0x466835, value);
            else if (RBDamagePotatoMine.IsChecked == true) PVZ.Memory.WriteByte(0x466A61, value);
            else if (RBDamageCobCannon.IsChecked == true) PVZ.Memory.WriteByte(0x46D839, value);
            else if (RBDamageJackH.IsChecked == true) PVZ.Memory.WriteByte(0x526C51, value);
            else if (RBDamageJack.IsChecked == true) PVZ.Memory.WriteByte(0x526C6D, value);
        }

        private void NudBound_ValueChanged(object sender, EventArgs e)
        {
            if (!IsLoaded) return;
            if (RBDamageExpWallNut.IsChecked == true) PVZ.Memory.WriteByte(0x462E7C, (byte)NudBound.Value);
            else if (RBDamageCherryBomb.IsChecked == true) PVZ.Memory.WriteByte(0x4667D9, (byte)NudBound.Value);
            else if (RBDamageDoomShroom.IsChecked == true) PVZ.Memory.WriteByte(0x466837, (byte)NudBound.Value);
            else if (RBDamagePotatoMine.IsChecked == true) PVZ.Memory.WriteByte(0x466A63, (byte)NudBound.Value);
            else if (RBDamageCobCannon.IsChecked == true) PVZ.Memory.WriteByte(0x46D83B, (byte)NudBound.Value);
            else if (RBDamageJackH.IsChecked == true) PVZ.Memory.WriteByte(0x526C53, (byte)NudBound.Value);
            else if (RBDamageJack.IsChecked == true) PVZ.Memory.WriteByte(0x526C6F, (byte)NudBound.Value);
        }

        private void NudRadius_ValueChanged(object sender, EventArgs e)
        {
            if (!IsLoaded) return;
            if (RBDamageExpWallNut.IsChecked == true) PVZ.Memory.WriteByte(0x462E7E, (byte)NudRadius.Value);
            else if (RBDamageCherryBomb.IsChecked == true) PVZ.Memory.WriteByte(0x4667DB, (byte)NudRadius.Value);
            else if (RBDamageDoomShroom.IsChecked == true) PVZ.Memory.WriteByte(0x466839, (byte)NudRadius.Value);
            else if (RBDamagePotatoMine.IsChecked == true) PVZ.Memory.WriteByte(0x466A65, (byte)NudRadius.Value);
            else if (RBDamageCobCannon.IsChecked == true) PVZ.Memory.WriteByte(0x46D83D, (byte)NudRadius.Value);
            else if (RBDamageJackH.IsChecked == true) PVZ.Memory.WriteByte(0x526C55, (byte)NudRadius.Value);
            else if (RBDamageJack.IsChecked == true) PVZ.Memory.WriteByte(0x526C71, (byte)NudRadius.Value);
        }

        public int scale = 100;

        private void Window_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (e.Delta > 0) scale += 5; else scale -= 5;
                scale = Math.Max(10, scale); scale = Math.Min(300, scale);
                System.Windows.UIElement con = Content as System.Windows.UIElement;
                con.RenderTransform = new System.Windows.Media.ScaleTransform(scale / 100.0, scale / 100.0);
                Height = 470.0 * scale / 100; Width = 400.0 * scale / 100;
            }
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e) { Lang.ChangeLanguage(Content); }

        private void TBDamageProjectile_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(0x69F1C8 + CBDamageProjectile.SelectedIndex * 0xC, Convert.ToInt32(TBDamageProjectile.Text)); }
        private void CBDamageProjectile_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (IsLoaded) TBDamageProjectile.Text = PVZ.Memory.ReadInteger(0x69F1C8 + CBDamageProjectile.SelectedIndex * 0xC).ToString(); }

        private int[] Damages = { 0x532FDC, 0x532B9C, 0x41D931, 0x4614DD, 0x532493, 0x4607A9, 0x45EDEF };

        private void CBDamageSpecial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (CBDamageSpecial.SelectedIndex == 3 || CBDamageSpecial.SelectedIndex == 4)
                TBDamageSpecial.Text = PVZ.Memory.ReadByte(Damages[CBDamageSpecial.SelectedIndex]).ToString();
            else
                TBDamageSpecial.Text = PVZ.Memory.ReadInteger(Damages[CBDamageSpecial.SelectedIndex]).ToString();
        }

        private void TBDamageSpecial_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!DealKeyDown(sender, e)) return;
            if (CBDamageSpecial.SelectedIndex == 3 || CBDamageSpecial.SelectedIndex == 4)
                PVZ.Memory.WriteByte(Damages[CBDamageSpecial.SelectedIndex], Convert.ToByte(TBDamageSpecial.Text));
            else
                PVZ.Memory.WriteInteger(Damages[CBDamageSpecial.SelectedIndex], Convert.ToInt32(TBDamageSpecial.Text));
        }

        private void CBDamageZombie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (CBDamageZombie.SelectedIndex == 0) TBDamageZombie.Text = ((sbyte)PVZ.Memory.ReadByte(0x52FCF3)).ToString();
            else if (CBDamageZombie.SelectedIndex == 1) TBDamageZombie.Text = PVZ.Memory.ReadByte(0x52FE14).ToString();
            else if (CBDamageZombie.SelectedIndex == 2) TBDamageZombie.Text = ((sbyte)PVZ.Memory.ReadByte(0x45EC66)).ToString();
        }

        private void TBDamageZombie_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!DealKeyDown(sender, e)) return;
            if (CBDamageZombie.SelectedIndex == 0) PVZ.Memory.WriteByte(0x52FCF3, (byte)(sbyte)Convert.ToSByte(TBDamageZombie.Text));
            else if (CBDamageZombie.SelectedIndex == 1) PVZ.Memory.WriteByte(0x52FE14, Convert.ToByte(TBDamageZombie.Text));
            else if (CBDamageZombie.SelectedIndex == 2) PVZ.Memory.WriteByte(0x45EC66, (byte)(sbyte)Convert.ToSByte(TBDamageZombie.Text));
        }

        private int[] DamageTimes = { 0x5309C7, 0x5309CE, 0x532741, 0x532400, 0x53241C, 0x532426, 0x53240B, 0x532415 };
        private void CBDamageTime_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (IsLoaded) TBDamageTime.Text = PVZ.Memory.ReadInteger(DamageTimes[CBDamageTime.SelectedIndex]).ToString(); }
        private void TBDamageTime_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(DamageTimes[CBDamageTime.SelectedIndex], Convert.ToInt32(TBDamageTime.Text)); }

        private void NudSceneGridRow_ValueChanged(object sender, EventArgs e) { if (IsLoaded) { int row = (int)NudSceneGridRow.Value; CBSceneRouteType.SelectedIndex = (int)PVZ.Lawn.GetRouteType(row); CBSceneGridType.SelectedIndex = (int)(PVZ.Lawn.GetGridType(row, (int)NudSceneGridColumn.Value) - 1); } }
        private void NudSceneGridColumn_ValueChanged(object sender, EventArgs e) { if (IsLoaded) CBSceneGridType.SelectedIndex = (int)(PVZ.Lawn.GetGridType((int)NudSceneGridRow.Value, (int)NudSceneGridColumn.Value) - 1); }
        private void CBSceneGridType_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBSceneGridType.IsMouseOver) PVZ.Lawn.SetGridType((int)NudSceneGridRow.Value, (int)NudSceneGridColumn.Value, (PVZ.LawnType)(CBSceneGridType.SelectedIndex + 1)); }
        private void CBSceneRouteType_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBSceneRouteType.IsMouseOver) PVZ.Lawn.SetRouteType((int)NudSceneGridRow.Value, (PVZ.RouteType)CBSceneRouteType.SelectedIndex); }

        private void RBPoolSceneNormal_Click(object sender, System.Windows.RoutedEventArgs e) { PVZ.Memory.WriteByte(0x40A668, 0xAE); PVZ.Memory.WriteByte(0x40A66E, 0xAE); PVZ.Memory.WriteByte(0x40A674, 0x8E); PVZ.Memory.WriteByte(0x40A67A, 0x8E); PVZ.Memory.WriteByte(0x40A680, 0xAE); PVZ.Memory.WriteByte(0x40A686, 0xAE); }
        private void RBPoolSceneReverse_Click(object sender, System.Windows.RoutedEventArgs e) { PVZ.Memory.WriteByte(0x40A668, 0x8E); PVZ.Memory.WriteByte(0x40A66E, 0x8E); PVZ.Memory.WriteByte(0x40A674, 0xAE); PVZ.Memory.WriteByte(0x40A67A, 0xAE); PVZ.Memory.WriteByte(0x40A680, 0x8E); PVZ.Memory.WriteByte(0x40A686, 0x8E); }
        private void RBPoolSceneFlood_Click(object sender, System.Windows.RoutedEventArgs e) { PVZ.Memory.WriteByte(0x40A668, 0x8E); PVZ.Memory.WriteByte(0x40A66E, 0x8E); PVZ.Memory.WriteByte(0x40A674, 0x8E); PVZ.Memory.WriteByte(0x40A67A, 0x8E); PVZ.Memory.WriteByte(0x40A680, 0x8E); PVZ.Memory.WriteByte(0x40A686, 0xAE); }
        private void RBPoolSceneLand_Click(object sender, System.Windows.RoutedEventArgs e) { PVZ.Memory.WriteByte(0x40A668, 0xAE); PVZ.Memory.WriteByte(0x40A66E, 0xAE); PVZ.Memory.WriteByte(0x40A674, 0xAE); PVZ.Memory.WriteByte(0x40A67A, 0xAE); PVZ.Memory.WriteByte(0x40A680, 0xAE); PVZ.Memory.WriteByte(0x40A686, 0xAE); }

        private void MIAdvicePot_Click(object sender, System.Windows.RoutedEventArgs e) { if (MIAdvicePot.IsChecked == true) { PVZ.Memory.WriteByte(0x41CD19, 235); PVZ.Memory.WriteByte(0x4857A8, 235); } else { PVZ.Memory.WriteByte(0x41CD19, 116); PVZ.Memory.WriteByte(0x4857A8, 116); } }
        private void CBUnsoddedAsRoof_Click(object sender, System.Windows.RoutedEventArgs e) { if (CBDefaultUnsodded.IsChecked != true) { CBDefaultUnsodded.IsChecked = true; PVZ.DefaultUnsodded(); } }

        private int[] GroundPropetys = { 0x413BA4, 0x413BAC, 0x413BBA, 0x422BD9, 0x422C6B, 0x466644, 0x426FCE, 0x4277D8, 0x52A8B6, 0x52A8D2, 0x41F7A2, 0x466887 };
        private void TBGroundPropety_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) { PVZ.Memory.WriteInteger(GroundPropetys[CBGroundPropety.SelectedIndex], Convert.ToInt32(TBGroundPropety.Text)); if (CBGroundPropety.SelectedIndex == 1) PVZ.Memory.WriteInteger(0x413BB1, Convert.ToInt32(TBGroundPropety.Text)); } }
        private void CBGroundPropety_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBGroundPropety.IsMouseOver) TBGroundPropety.Text = PVZ.Memory.ReadInteger(GroundPropetys[CBGroundPropety.SelectedIndex]).ToString(); }

        private void TBWharkaZombieSpawnCount_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(0x426193, Convert.ToInt32(TBWharkaZombieSpawnCount.Text)); }
        private void NudWharkaZombieZombieSpeed_ValueChanged(object sender, EventArgs e) { if (IsLoaded) PVZ.Memory.WriteByte(0x42630B, (byte)NudWharkaZombieZombieSpeed.Value); }
        private void NudWharkaZombieMinGraveCount_ValueChanged(object sender, EventArgs e) { if (IsLoaded) PVZ.Memory.WriteInteger(0x426044, (int)NudWharkaZombieMinGraveCount.Value); }
        private void TBWharkaZombieSpawnSpeed_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) { if (PVZ.Memory.ReadInteger(0x426559) == 412001000) PVZ.Memory.WriteByte(0x426559, 134); PVZ.Memory.WriteInteger(0x42655A, Convert.ToInt32(TBWharkaZombieSpawnSpeed.Text)); } }

        private void SIcetrace_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            for (int index = 0; index <= 5; index++)
            {
                switch (CBLockIcetrace.SelectedIndex)
                {
                    case 0: PVZ.Icetrace.SetX(index, (int)(int)SIcetrace.Value); PVZ.Icetrace.SetDisapperaCountdown(index, 1000); break;
                    case 1: if (index == 2 || index == 3) break; PVZ.Icetrace.SetX(index, (int)(int)SIcetrace.Value); PVZ.Icetrace.SetDisapperaCountdown(index, 1000); break;
                    case 2: if (index == 0) { PVZ.Icetrace.SetX(0, (int)(int)SIcetrace.Value); PVZ.Icetrace.SetDisapperaCountdown(0, 1000); } break;
                    case 3: if (index == 1) { PVZ.Icetrace.SetX(1, (int)(int)SIcetrace.Value); PVZ.Icetrace.SetDisapperaCountdown(1, 1000); } break;
                    case 4: if (index == 2) { PVZ.Icetrace.SetX(2, (int)(int)SIcetrace.Value); PVZ.Icetrace.SetDisapperaCountdown(2, 1000); } break;
                    case 5: if (index == 3) { PVZ.Icetrace.SetX(3, (int)(int)SIcetrace.Value); PVZ.Icetrace.SetDisapperaCountdown(3, 1000); } break;
                    case 6: if (index == 4) { PVZ.Icetrace.SetX(4, (int)(int)SIcetrace.Value); PVZ.Icetrace.SetDisapperaCountdown(4, 1000); } break;
                    case 7: if (index == 5) { PVZ.Icetrace.SetX(5, (int)(int)SIcetrace.Value); PVZ.Icetrace.SetDisapperaCountdown(5, 1000); } break;
                }
            }
        }

        private int[] ItemValues = { 0x430A46, 0x430A52, 0x430A5C, 0x4309F0, 0x4309FC, 0x430A03 };
        private void TBItemValue_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(ItemValues[CBItemValue.SelectedIndex], Convert.ToInt32(TBItemValue.Text)); }
        private void CBItemValue_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBItemValue.IsMouseOver) TBItemValue.Text = PVZ.Memory.ReadInteger(ItemValues[CBItemValue.SelectedIndex]).ToString(); }

        private byte GetCoinTypeValue(int value) { return (byte)Enum.GetValues(typeof(PVZ.CoinType)).GetValue(value); }

        private void CBDropItem_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBDropItem.IsMouseOver) PVZ.Memory.WriteInteger(0x413BD9, GetCoinTypeValue(CBDropItem.SelectedIndex)); }
        private void CBSunnyDayDropItem_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBSunnyDayDropItem.IsMouseOver) PVZ.Memory.WriteInteger(0x413BE0, GetCoinTypeValue(CBSunnyDayDropItem.SelectedIndex)); }
        private void CBDropPlantType_SelectionChanged(object sender, SelectionChangedEventArgs e) { PVZ.Memory.WriteInteger(0x42FFB9, CBDropPlantType.SelectedIndex); }

        private void RBZombieDropNormal_Click(object sender, System.Windows.RoutedEventArgs e) { PVZ.Memory.WriteByte(0x530275, 117); PVZ.Memory.WriteInteger(0x41CF10, -914024332); }
        private void RBZombieWillDrop_Click(object sender, System.Windows.RoutedEventArgs e) { PVZ.Memory.WriteByte(0x530275, 112); }
        private void RBZombieMightDrop_Click(object sender, System.Windows.RoutedEventArgs e) { PVZ.Memory.WriteInteger(0x41CF10, -914024213); }

        private void CBZombieWillDropItem1_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBZombieWillDropItem1.IsMouseOver) PVZ.Memory.WriteByte(0x53028D, GetCoinTypeValue(CBZombieWillDropItem1.SelectedIndex)); }
        private void CBZombieWillDropItem2_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBZombieWillDropItem2.IsMouseOver) PVZ.Memory.WriteByte(0x53029B, GetCoinTypeValue(CBZombieWillDropItem2.SelectedIndex)); }
        private void CBZombieWillDropItem3_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBZombieWillDropItem3.IsMouseOver) PVZ.Memory.WriteByte(0x5302AF, GetCoinTypeValue(CBZombieWillDropItem3.SelectedIndex)); }
        private void CBZombieWillDropItem4_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBZombieWillDropItem4.IsMouseOver) PVZ.Memory.WriteByte(0x5302C0, GetCoinTypeValue(CBZombieWillDropItem4.SelectedIndex)); }
        private void CBZombieMightDroItem1_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBZombieMightDroItem1.IsMouseOver) PVZ.Memory.WriteByte(0x41CFE6, GetCoinTypeValue(CBZombieMightDroItem1.SelectedIndex)); }
        private void CBZombieMightDroItem2_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBZombieMightDroItem2.IsMouseOver) PVZ.Memory.WriteByte(0x41CFF6, GetCoinTypeValue(CBZombieMightDroItem2.SelectedIndex)); }
        private void CBZombieMightDroItem3_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBZombieMightDroItem3.IsMouseOver) PVZ.Memory.WriteByte(0x41D006, GetCoinTypeValue(CBZombieMightDroItem3.SelectedIndex)); }

        private void CBMarigoldDrop1_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBMarigoldDrop1.IsMouseOver) { PVZ.Memory.WriteByte(0x45FAFC, (byte)(218 + GetCoinTypeValue(CBMarigoldDrop1.SelectedIndex))); PVZ.Memory.WriteByte(0x45FAFF, (byte)(100 - GetCoinTypeValue(CBMarigoldDrop1.SelectedIndex))); } }
        private void CBMarigoldDrop2_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBMarigoldDrop2.IsMouseOver) PVZ.Memory.WriteByte(0x45FB0B, GetCoinTypeValue(CBMarigoldDrop2.SelectedIndex)); }
        private void NudPMarigoldDrop_ValueChanged(object sender, EventArgs e) { if (IsLoaded) PVZ.Memory.WriteByte(0x45FB07, (byte)NudPMarigoldDrop.Value); }

        private int[,] ConveyorBeltCards = {
            {0x422E42,0x422E4E,0x422E6E,0x422E7A,0,0,0}, {0x422EA0,0x422EB0,0x422EBC,0x422ED0,0,0,0},
            {0x422EF6,0x422F02,0x422F0E,0x422F22,0x422F2E,0x422F3A,0}, {0x422F60,0x422F6C,0x422F78,0x422F84,0x422F94,0x422FA0,0x422FAC},
            {0x422FCD,0x422FDD,0x422FF5,0x423001,0x42300D,0,0}, {0x42308F,0x42309F,0,0,0,0,0},
            {0x423110,0x423120,0x423130,0x423140,0,0,0}, {0x4230CB,0x4230D7,0x4230EB,0,0,0,0},
            {0x423059,0x42306D,0x423075,0,0,0,0}, {0x423160,0x423170,0x423180,0x423190,0,0,0},
            {0x4231C4,0x4231D0,0x4231E0,0x4231EC,0,0,0}, {0x42320A,0x42321A,0x42322E,0x423246,0,0,0},
            {0x42326A,0x423272,0x42327E,0x42328A,0x42329A,0,0}
        };

        private void CBConveyorBeltLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            CBBeltCard1.IsEnabled = false; CBBeltCard2.IsEnabled = false; CBBeltCard3.IsEnabled = false; CBBeltCard4.IsEnabled = false;
            CBBeltCard5.IsEnabled = false; CBBeltCard6.IsEnabled = false; CBBeltCard7.IsEnabled = false;
            if (CBConveyorBeltLevel.SelectedIndex == 8) { PVZ.Memory.WriteInteger(0x423059, 3); PVZ.Memory.WriteByte(0x42305E, 0x44); }
            int cardcount = int.Parse(((ComboBoxItem)CBConveyorBeltLevel.SelectedItem).Tag.ToString());
            for (int i = 0; i < Math.Min(cardcount, 7); i++)
            {
                var cb = (ComboBox)FindName($"CBBeltCard{i + 1}");
                if (cb != null) { cb.IsEnabled = true; cb.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards[CBConveyorBeltLevel.SelectedIndex, i]); }
            }
        }

        private void CBBeltCard_SelectionChanged(object sender, SelectionChangedEventArgs e, int cardIndex)
        {
            var cb = sender as ComboBox;
            if (cb != null && cb.IsMouseOver)
                PVZ.Memory.WriteInteger(ConveyorBeltCards[CBConveyorBeltLevel.SelectedIndex, cardIndex], cb.SelectedIndex);
        }
        private void CBBeltCard1_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBBeltCard_SelectionChanged(sender, e, 0); }
        private void CBBeltCard2_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBBeltCard_SelectionChanged(sender, e, 1); }
        private void CBBeltCard3_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBBeltCard_SelectionChanged(sender, e, 2); }
        private void CBBeltCard4_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBBeltCard_SelectionChanged(sender, e, 3); }
        private void CBBeltCard5_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBBeltCard_SelectionChanged(sender, e, 4); }
        private void CBBeltCard6_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBBeltCard_SelectionChanged(sender, e, 5); }
        private void CBBeltCard7_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBBeltCard_SelectionChanged(sender, e, 6); }

        private void CBBeltCardLilyPad_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBBeltCardLilyPad.IsMouseOver) PVZ.Memory.WriteByte(0x422E2B, (byte)CBBeltCardLilyPad.SelectedIndex); }
        private void CBBeltCardJalapeno_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBBeltCardJalapeno.IsMouseOver) PVZ.Memory.WriteByte(0x422E2E, (byte)CBBeltCardJalapeno.SelectedIndex); }

        private void CBEnableConveyorBelt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ResumeConveyorBelt();
            if (CBEnableConveyorBelt.IsChecked == true)
            {
                if (CBConveyorBeltLevel.SelectedIndex != -1) PVZ.Memory.WriteByte(0x41BECE, 0x90);
                int[] beltAddrs = { 0x422E2F, 0x422E89, 0x422EE3, 0x422F4D, 0x422FC7, 0x423089, 0x423106, 0x4230BC, 0x423051, 0x42315A, 0x4231A9, 0x4231FF, 0x423253 };
                if (CBConveyorBeltLevel.SelectedIndex >= 0 && CBConveyorBeltLevel.SelectedIndex < beltAddrs.Length)
                    PVZ.Memory.WriteByte(beltAddrs[CBConveyorBeltLevel.SelectedIndex], 0x70);
            }
            else PVZ.Memory.WriteByte(0x41BECE, 0xC3);
        }

        private static void ResumeConveyorBelt()
        {
            byte[] vals = { 0x75, 0x75, 0x75, 0x75, 0x74, 0x75, 0x74, 0x74, 0x74, 0x74, 0x75, 0x75, 0x75 };
            int[] addrs = { 0x422E2F, 0x422E89, 0x422EE3, 0x422F4D, 0x422FC7, 0x423051, 0x423089, 0x4230BC, 0x423106, 0x42315A, 0x4231A9, 0x4231FF, 0x423253 };
            for (int i = 0; i < addrs.Length; i++) PVZ.Memory.WriteByte(addrs[i], vals[i]);
        }

        private void CBLSLimitPlant_SelectionChanged(object sender, SelectionChangedEventArgs e, int plant)
        {
            var cb = sender as ComboBox;
            if (cb == null || !cb.IsMouseOver) return;
            int p = cb.SelectedIndex;
            int[] addrs = { 0x482E1A + plant * 5, 0x482F62 + plant * 5, 0x4832AA + plant * 5, 0x484A3D + plant * 5, 0x486B8C + plant * 5, 0x484622 + plant * 5 };
            foreach (int a in addrs) PVZ.Memory.WriteByte(a, (byte)p);
        }
        private void CBLSLimitPlant1_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBLSLimitPlant_SelectionChanged(sender, e, 0); }
        private void CBLSLimitPlant2_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBLSLimitPlant_SelectionChanged(sender, e, 1); }
        private void CBLSLimitPlant3_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBLSLimitPlant_SelectionChanged(sender, e, 2); }
        private void CBLSLimitPlant4_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBLSLimitPlant_SelectionChanged(sender, e, 3); }
        private void CBLSLimitPlant5_SelectionChanged(object sender, SelectionChangedEventArgs e) { CBLSLimitPlant_SelectionChanged(sender, e, 4); }

        private void NudPlantLimitLine_ValueChanged(object sender, EventArgs e) { if (IsLoaded) { int line = (int)NudPlantLimitLine.Value; PVZ.Memory.WriteByte(0x425583, (byte)(line - 1)); PVZ.Memory.WriteInteger(0x425392, 20 + 80 * line); } }
        private void NudZombieLimitLine1_ValueChanged(object sender, EventArgs e) { if (IsLoaded) { int line = (int)NudZombieLimitLine1.Value; PVZ.Memory.WriteByte(0x4255C4, (byte)line); PVZ.Memory.WriteInteger(0x4253C7, 20 + 80 * line); } }
        private void NudZombieLimitLine2_ValueChanged(object sender, EventArgs e) { if (IsLoaded) { int line = (int)NudZombieLimitLine2.Value; PVZ.Memory.WriteByte(0x4255DD, (byte)line); PVZ.Memory.WriteInteger(0x4253F7, 20 + 80 * line); } }
        private void NudZombieLimitLine3_ValueChanged(object sender, EventArgs e) { if (IsLoaded) { int line = (int)NudZombieLimitLine3.Value; PVZ.Memory.WriteByte(0x4255A9, (byte)line); PVZ.Memory.WriteInteger(0x425416, 20 + 80 * line); } }

        private void TB95NewsZombieBodyHp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(0x4002FF, Convert.ToInt32(TB95NewsZombieBodyHp.Text)); }
        private void TB95FlagZombieBodyHp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(0x40049F, Convert.ToInt32(TB95FlagZombieBodyHp.Text)); }
        private void TB95NormalZombieBodyHp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(0x4004AB, Convert.ToInt32(TB95FlagZombieBodyHp.Text)); }
        private void TB95HypnotizedZombieHpAdd_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) { PVZ.Memory.WriteInteger(0x400384, Convert.ToInt32(TB95HypnotizedZombieHpAdd.Text)); PVZ.Memory.WriteInteger(0x400396, Convert.ToInt32(TB95HypnotizedZombieHpAdd.Text)); } }
        private void TB95TallnutCounterattackHp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(0x400464, Convert.ToInt32(TB95TallnutCounterattackHp.Text)); }
        private void TB95BloverDamage_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) { byte b = (byte)Math.Min(127, Convert.ToDouble(TB95BloverDamage.Text)); PVZ.Memory.WriteInteger(0x40075E, b); PVZ.Memory.WriteByte(0x400766, b); } }
        private void TB95IceLevelSunCondition_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(0x400938, Convert.ToInt32(TB95IceLevelSunCondition.Text)); }
        private void TB95NewspaperAngrySpeedMultiple_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteDouble(0x4009BA, Convert.ToDouble(TB95NewspaperAngrySpeedMultiple.Text)); }
        private void TB95JalapenoDamage_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Memory.WriteInteger(0x466520, Convert.ToInt32(TB95JalapenoDamage.Text)); }
    }
}
