using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ITrainerExtension;
using PVZClass;

namespace PVZWPFTrainner
{
    public partial class MonitorWindow : Window
    {
        private readonly HpTrackWindow tracker = new HpTrackWindow();
        private readonly System.Windows.Threading.DispatcherTimer[] Timers = new System.Windows.Threading.DispatcherTimer[8];
        private readonly List<CheckBox> CheckBoxes = new List<CheckBox>();
        private int prezombienum = 0;
        private int preselzombieid = 0;
        private PVZ.Zombie zombie;
        private int preplantnum = 0;
        private int preselplantid = 0;
        private PVZ.Plant plant;
        private int precoinnum = 0;
        private int preselcoinid = 0;
        private PVZ.Coin coin;
        private int pregriditemnum = 0;
        private int preselgriditemid = 0;
        private PVZ.Griditem griditem;
        private PVZ.Crater crater;
        private PVZ.Vase vase;
        private int precardmnum = 0;
        private int preselcardid = 0;
        private PVZ.CardSlot.SeedCard card;

        private bool DealKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) { ((TextBox)sender).Text = "0"; e.Handled = true; }
            else if (e.Key == Key.V) { e.Handled = true; }
            return e.Key == Key.Enter;
        }

        public MonitorWindow()
        {
            InitializeComponent();
            for (int i = 0; i <= 7; i++) { Timers[i] = new System.Windows.Threading.DispatcherTimer(); Timers[i].Interval = new TimeSpan(100); }
            Timers[0].Tick += Timer1Tick; Timers[1].Tick += Timer2Tick; Timers[2].Tick += Timer3Tick; Timers[3].Tick += Timer4Tick;
            Timers[4].Tick += Timer5Tick; Timers[5].Tick += Timer6Tick; Timers[6].Tick += Timer7Tick; Timers[7].Tick += Timer8Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <= 8; i++)
            {
                var panel = new StackPanel();
                for (int j = 0; j <= 5; j++)
                {
                    var cbox = new DarkStyle.DarkCheckBox();
                    cbox.Click += (_sender, _e) =>
                    {
                        int index = CheckBoxes.IndexOf((CheckBox)_sender);
                        PVZ.Miscellaneous.SetCrater(index % 6, (int)Math.Floor(index / 6.0), ((CheckBox)_sender).IsChecked == true);
                    };
                    CheckBoxes.Add(cbox);
                    panel.Children.Add(cbox);
                }
                SPCrater.Children.Add(panel);
            }
            Lang.ChangeLanguage(Content);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { try { DragMove(); } catch (InvalidOperationException) { } }

        private void Window_Closed(object sender, EventArgs e)
        {
            tracker.Close();
            if (Application.Current.MainWindow != null) ((MainWindow)Application.Current.MainWindow).BtnMonitor.IsEnabled = true;
        }

        private void TCMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            try { foreach (TabItem item in TCMain.Items) Timers[item.TabIndex].Stop(); Timers[((TabItem)TCMain.SelectedItem).TabIndex].Start(); } catch { }
        }

        private void Timer1Tick(object sender, EventArgs e)
        {
            TBMouseInGameArea.Text = PVZ.Mouse.InGameArea ? "是" : "否";
            if (Lang.Id == 1) TBMouseInGameArea.Text = PVZ.Mouse.InGameArea.ToString();
            TBMouseX.Text = PVZ.Mouse.X.ToString();
            TBMouseY.Text = PVZ.Mouse.Y.ToString();
            TBMouseRow.Text = PVZ.MousePointer.Row.ToString();
            TBMouseColumn.Text = PVZ.MousePointer.Column.ToString();
            TBMouseStateLeft.Foreground = Brushes.White; TBMouseStateMid.Foreground = Brushes.White; TBMouseStateRight.Foreground = Brushes.White;
            var cs = PVZ.Mouse.ClickState;
            if (cs == PVZ.MouseClickState.LButton || cs == PVZ.MouseClickState.LRButton || cs == PVZ.MouseClickState.LMidButton || cs == PVZ.MouseClickState.LRMidButton) TBMouseStateLeft.Foreground = Brushes.Red;
            if (cs == PVZ.MouseClickState.MidButton || cs == PVZ.MouseClickState.LMidButton || cs == PVZ.MouseClickState.RMidButton || cs == PVZ.MouseClickState.LRMidButton) TBMouseStateMid.Foreground = Brushes.Red;
            if (cs == PVZ.MouseClickState.RButton || cs == PVZ.MouseClickState.LRButton || cs == PVZ.MouseClickState.RMidButton || cs == PVZ.MouseClickState.LRMidButton) TBMouseStateRight.Foreground = Brushes.Red;
        }

        private void Timer2Tick(object sender, EventArgs e)
        {
            if (PVZ.ZombiesCount != prezombienum)
            {
                if (prezombienum != 0) LBZombies_SelectionChanged(null, null);
                LBZombies.Items.Clear();
                foreach (var z in PVZ.AllZombies) { var tb = new TextBlock { Text = (z.Id & 0xFFFF).ToString(), Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center }; LBZombies.Items.Add(tb); }
                prezombienum = PVZ.ZombiesCount;
                foreach (TextBlock item in LBZombies.Items) if (item.Text == preselzombieid.ToString()) LBZombies.SelectedItem = item;
            }
            if (LBZombies.SelectedIndex >= 0)
            {
                zombie = new PVZ.Zombie(Convert.ToInt32(((TextBlock)LBZombies.SelectedItem).Text));
                TBZombieType.Text = zombie.Type.GetDescription(); TBZombieId.Text = "id = " + zombie.Id;
                if (Lang.Id == 1) TBZombieType.Text = zombie.Type.ToString();
                if (!TBZombieX.IsFocused) TBZombieX.Text = zombie.X.ToString();
                if (!TBZombieY.IsFocused) TBZombieY.Text = zombie.Y.ToString();
                if (!NudZombieRow.IsMouseOver) { NudZombieRow.IgnoreAssign = true; NudZombieRow.Value = zombie.Row + 1; NudZombieRow.IgnoreAssign = false; }
                if (!TBZombieState.IsFocused) TBZombieState.Text = zombie.State.ToString();
                if (!TBZombieBodyHp.IsFocused) TBZombieBodyHp.Text = zombie.BodyHP.ToString();
                if (!TBZombieA1Hp.IsFocused) TBZombieA1Hp.Text = zombie.AccessoriesType1HP.ToString();
                if (!TBZombieA2Hp.IsFocused) TBZombieA2Hp.Text = zombie.AccessoriesType2HP.ToString();
                if (!CBZombieVisible.IsMouseOver) CBZombieVisible.IsChecked = !zombie.Visible;
                if (!CBZombieHypnotized.IsMouseOver) CBZombieHypnotized.IsChecked = zombie.Hypnotized;
                if (!CBZombieBlowaway.IsMouseOver) CBZombieBlowaway.IsChecked = zombie.Blowaway;
                if (!CBZombieDying.IsMouseOver) CBZombieDying.IsChecked = zombie.Dying;
                if (!CBZombieGarlicBited.IsMouseOver) CBZombieGarlicBited.IsChecked = zombie.GarlicBited;
                if (!CBZombieExist.IsMouseOver) CBZombieExist.IsChecked = zombie.Exist;
                if (!SZombieDecelerate.IsMouseOver) SZombieDecelerate.Value = zombie.DecelerateCountdown / 100.0;
                if (!SZombieFixed.IsMouseOver) SZombieFixed.Value = zombie.FixedCountdown / 100.0;
                if (!SZombieFrozen.IsMouseOver) SZombieFrozen.Value = zombie.FrozenCountdown / 100.0;
            }
        }

        private void Timer3Tick(object sender, EventArgs e)
        {
            if (PVZ.PlantsCount != preplantnum)
            {
                if (preplantnum != 0) LBPlants_SelectionChanged(null, null);
                LBPlants.Items.Clear();
                foreach (var p in PVZ.AllPlants) { var tb = new TextBlock { Text = (p.Id & 0xFFFF).ToString(), Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center }; LBPlants.Items.Add(tb); }
                preplantnum = PVZ.PlantsCount;
                foreach (TextBlock item in LBPlants.Items) if (item.Text == preselplantid.ToString()) LBPlants.SelectedItem = item;
            }
            if (LBPlants.SelectedIndex >= 0)
            {
                plant = new PVZ.Plant(Convert.ToInt32(((TextBlock)LBPlants.SelectedItem).Text));
                TBPlantType.Text = plant.Type.GetDescription(); if (Lang.Id == 1) TBPlantType.Text = plant.Type.ToString();
                TBPlantId.Text = "id = " + plant.Id;
                if (!TBPlantX.IsFocused) TBPlantX.Text = plant.X.ToString();
                if (!TBPlantY.IsFocused) TBPlantY.Text = plant.Y.ToString();
                if (!TBPlantRow.IsFocused) TBPlantRow.Text = plant.Row.ToString();
                if (!TBPlantColumn.IsFocused) TBPlantColumn.Text = plant.Column.ToString();
                if (!TBPlantState.IsFocused) TBPlantState.Text = plant.State.ToString();
                if (!TBPlantHp.IsFocused) TBPlantHp.Text = plant.Hp.ToString();
                if (!CBPlantVisible.IsMouseOver) CBPlantVisible.IsChecked = !plant.Visible;
                if (!CBPlantAggressive.IsMouseOver) CBPlantAggressive.IsChecked = plant.Aggressive;
                if (!CBPlantSquash.IsMouseOver) CBPlantSquash.IsChecked = plant.Squash;
                if (!CBPlantSleeping.IsMouseOver) CBPlantSleeping.IsChecked = plant.Sleeping;
                if (!CBPlantExist.IsMouseOver) CBPlantExist.IsChecked = plant.Exist;
                if (!SPlantProduct.IsMouseOver) SPlantProduct.Value = plant.ShootOrProductCountdown / 100.0;
                if (!SPlantAttribute.IsMouseOver) SPlantAttribute.Value = plant.AttributeCountdown / 100.0;
                if (!SPlantShooting.IsMouseOver) SPlantShooting.Value = plant.ShootingCountdown / 100.0;
                if (!TBPlantProductInterval.IsFocused) TBPlantProductInterval.Text = plant.ShootOrProductInterval.ToString();
            }
        }

        private void Timer4Tick(object sender, EventArgs e)
        {
            if (PVZ.CoinsCount != precoinnum)
            {
                if (precoinnum != 0) LBCoins_SelectionChanged(null, null);
                LBCoins.Items.Clear();
                foreach (var c in PVZ.AllCoins) { var tb = new TextBlock { Text = (c.Id & 0xFFFF).ToString(), Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center }; LBCoins.Items.Add(tb); }
                precoinnum = PVZ.CoinsCount;
                foreach (TextBlock item in LBCoins.Items) if (item.Text == preselcoinid.ToString()) LBCoins.SelectedItem = item;
            }
            if (LBCoins.SelectedIndex >= 0)
            {
                coin = new PVZ.Coin(Convert.ToInt32(((TextBlock)LBCoins.SelectedItem).Text));
                TBCoinType.Text = coin.Type.GetDescription(); if (Lang.Id == 1) TBCoinType.Text = coin.Type.ToString();
                TBCoinId.Text = "id = " + coin.Id;
                if (!TBCoinX.IsFocused) TBCoinX.Text = coin.X.ToString();
                if (!TBCoinY.IsFocused) TBCoinY.Text = coin.Y.ToString();
                if (!TBCoinSize.IsFocused) TBCoinSize.Text = coin.Size.ToString();
                if (!CBCoinCard.IsFocused) CBCoinCard.SelectedIndex = (int)coin.CardType;
                if (!CBCoinVisible.IsMouseOver) CBCoinVisible.IsChecked = !coin.Visible;
                if (!CBCoinCollected.IsMouseOver) CBCoinCollected.IsChecked = coin.Collected;
                if (!CBCoinHalo.IsMouseOver) CBCoinHalo.IsChecked = coin.Halo;
            }
        }

        private void Timer5Tick(object sender, EventArgs e)
        {
            if (PVZ.GriditemsCount != pregriditemnum)
            {
                if (pregriditemnum != 0) LBGriditems_SelectionChanged(null, null);
                LBGriditems.Items.Clear();
                foreach (var g in PVZ.AllGriditems) { var tb = new TextBlock { Text = (g.Id & 0xFFFF).ToString(), Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center }; LBGriditems.Items.Add(tb); }
                pregriditemnum = PVZ.GriditemsCount;
                foreach (TextBlock item in LBGriditems.Items) if (item.Text == preselgriditemid.ToString()) LBGriditems.SelectedItem = item;
            }
            if (LBGriditems.SelectedIndex >= 0)
            {
                crater = new PVZ.Crater(Convert.ToInt32(((TextBlock)LBGriditems.SelectedItem).Text));
                vase = new PVZ.Vase(crater.BaseAddress);
                griditem = crater;
                TBGriditemType.Text = griditem.Type.GetDescription(); if (Lang.Id == 1) TBGriditemType.Text = griditem.Type.ToString();
                TBGriditemId.Text = "id = " + griditem.Id;
                if (!TBGriditemRow.IsFocused) TBGriditemRow.Text = griditem.Row.ToString();
                if (!TBGriditemColumn.IsFocused) TBGriditemColumn.Text = griditem.Column.ToString();
                if (!CBGriditemExist.IsMouseOver) CBGriditemExist.IsChecked = griditem.Exist;
                if (!SCraterDisappear.IsMouseOver) SCraterDisappear.Value = crater.DisappearCountdown / 100.0;
                if (!CBVaseSkin.IsFocused) CBVaseSkin.SelectedIndex = Math.Max((int)vase.Skin - 3, 0);
                if (!CBVaseContent.IsFocused) CBVaseContent.SelectedIndex = (int)vase.Content;
                if (!CBVaseZombie.IsFocused) CBVaseZombie.SelectedIndex = (int)vase.Zombie;
                if (!CBVasePlant.IsFocused) CBVasePlant.SelectedIndex = (int)vase.Plant;
                if (!TBVaseSun.IsFocused) TBVaseSun.Text = vase.Sun.ToString();
                if (!SVaseTransparent.IsMouseOver) SVaseTransparent.Value = vase.TransparentCountDown / 100.0;
            }
        }

        private void Timer6Tick(object sender, EventArgs e)
        {
            if (PVZ.CardSlot.CardNum != precardmnum)
            {
                if (precardmnum != 0) LBCards_SelectionChanged(null, null);
                LBCards.Items.Clear();
                for (int i = 0; i <= PVZ.CardSlot.CardNum - 1; i++) { card = PVZ.CardSlot.GetCard(i); var tb = new TextBlock { Text = card.Index.ToString(), Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center }; LBCards.Items.Add(tb); }
                precardmnum = PVZ.CardSlot.CardNum;
                foreach (TextBlock item in LBCards.Items) if (item.Text == precardmnum.ToString()) LBCards.SelectedItem = item;
            }
            if (!CBCardSlotVisible.IsMouseOver) CBCardSlotVisible.IsChecked = PVZ.CardSlot.Visible;
            if (!CBCardNum.IsMouseOver) CBCardNum.SelectedIndex = PVZ.CardSlot.CardNum;
            if (LBCards.SelectedIndex >= 0)
            {
                card = PVZ.CardSlot.GetCard(Convert.ToInt32(((TextBlock)LBCards.SelectedItem).Text));
                if (!TBCardX.IsFocused) TBCardX.Text = card.X.ToString();
                if (!TBCardY.IsFocused) TBCardY.Text = card.Y.ToString();
                if (!CBCardVisible.IsMouseOver) CBCardVisible.IsChecked = card.Visible;
                if (!CBCardEnable.IsMouseOver) CBCardEnable.IsChecked = card.Enable;
                if (!CBCardActive.IsMouseOver) CBCardActive.IsChecked = card.Active;
                if (!TBCoolDown.IsFocused) TBCoolDown.Text = card.CoolDownInterval.ToString();
                if (!CBCardType.IsFocused) CBCardType.SelectedIndex = (int)card.CardType;
                if (!CBCardTypeImitative.IsFocused) CBCardTypeImitative.SelectedIndex = (int)card.ImitativeCardType;
                if (!SCardCoolDowm.IsMouseOver) { SCardCoolDowm.Maximum = Math.Max(card.CoolDownInterval, SCardCoolDowm.Maximum) / 100.0 + 5; SCardCoolDowm.Value = card.CoolDown / 100.0; }
                if (!SCardBeltX.IsMouseOver) SCardBeltX.Value = card.ConveyorBeltX;
            }
        }

        private void Timer7Tick(object sender, EventArgs e)
        {
            for (int i = 0; i <= 8; i++) for (int j = 0; j <= 5; j++) CheckBoxes[i * 6 + j].IsChecked = PVZ.Miscellaneous.HaveCrater(j, i);
            if (!CBUpgradedRepeater.IsMouseOver) CBUpgradedRepeater.IsChecked = PVZ.Miscellaneous.UpgradedRepeater;
            if (!CBUpgradedFumeshroon.IsMouseOver) CBUpgradedFumeshroon.IsChecked = PVZ.Miscellaneous.UpgradedFumeshroon;
            if (!CBUpgradedTallnut.IsMouseOver) CBUpgradedTallnut.IsChecked = PVZ.Miscellaneous.UpgradedTallnut;
            if (!SAttributeTime.IsMouseOver) SAttributeTime.Value = PVZ.Miscellaneous.AttributeCountdown / 100.0;
            if (!TBLevelProcess.IsFocused) TBLevelProcess.Text = PVZ.Miscellaneous.LevelProcess.ToString();
            if (!TBLevelRound.IsFocused) TBLevelRound.Text = PVZ.Miscellaneous.Round.ToString();
            if (!CBSceneType.IsFocused) CBSceneType.SelectedIndex = (int)PVZ.Scene;
        }

        private void Timer8Tick(object sender, EventArgs e)
        {
            if (!CBMusicType.IsMouseOver) CBMusicType.SelectedIndex = Math.Max(0, (int)PVZ.Music.Type - 1);
            if (!CBINGAMEEnable.IsMouseOver) CBINGAMEEnable.IsChecked = PVZ.Music.INGAMEEnable;
            if (!CBINGAMEStart.IsMouseOver) CBINGAMEStart.IsChecked = PVZ.Music.INGAMEStart;
            TBMusicBPM.Text = PVZ.Music.Tempo.ToString();
            TBMusivTicksRow.Text = PVZ.Music.TicksRow.ToString();
        }

        #region Mouse
        private void TBoxMouseX_PreviewKeyDown(object sender, KeyEventArgs e) { DealKeyDown(sender, e); }
        private void BtnWMClick_Click(object sender, RoutedEventArgs e)
        {
            int ValueX = MainWindow.StrToInt(TBoxMouseX.Text); int ValueY = MainWindow.StrToInt(TBoxMouseY.Text);
            PVZ.Mouse.WMLClick((short)ValueX, (short)ValueY); TBoxMouseX.Text = ValueX.ToString(); TBoxMouseY.Text = ValueY.ToString();
        }
        #endregion

        #region Zombie
        private void TBZombieId_MouseDown(object sender, MouseButtonEventArgs e) { if (TBZombieId.Text != "id=") Clipboard.SetText(TBZombieId.Text.Substring(5)); }
        private void LBZombies_SelectionChanged(object sender, SelectionChangedEventArgs e) { try { preselzombieid = Convert.ToInt32(((TextBlock)LBZombies.SelectedItem).Text); } catch { } }
        private void TBZombieX_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && zombie != null) zombie.X = Convert.ToInt32(TBZombieX.Text); }
        private void TBZombieY_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && zombie != null) zombie.Y = Convert.ToInt32(TBZombieY.Text); }
        private void NudZombieRow_ValueChanged(object sender, EventArgs e) { if (zombie != null) zombie.Row = (int)NudZombieRow.Value - 1; }
        private void TBZombieState_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && zombie != null) zombie.State = Convert.ToInt32(TBZombieState.Text); }
        private void TBZombieBodyHp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && zombie != null) { zombie.BodyHP = Convert.ToInt32(TBZombieBodyHp.Text); if (zombie.BodyHP > zombie.MaxBodyHP) zombie.MaxBodyHP = zombie.BodyHP; } }
        private void TBZombieA1Hp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && zombie != null) { zombie.AccessoriesType1HP = Convert.ToInt32(TBZombieA1Hp.Text); if (zombie.AccessoriesType1HP > zombie.MaxAccessoriesType1HP) zombie.MaxAccessoriesType1HP = zombie.AccessoriesType1HP; } }
        private void TBZombieA2Hp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && zombie != null) { zombie.AccessoriesType2HP = Convert.ToInt32(TBZombieA2Hp.Text); if (zombie.AccessoriesType2HP > zombie.MaxAccessoriesType2HP) zombie.MaxAccessoriesType2HP = zombie.AccessoriesType2HP; } }
        private void CBZombieVisible_Click(object sender, RoutedEventArgs e) { if (zombie != null) zombie.Visible = CBZombieVisible.IsChecked != true; }
        private void CBZombieHypnotized_Click(object sender, RoutedEventArgs e) { if (zombie != null) zombie.Hypnotized = (CBZombieHypnotized.IsChecked == true); }
        private void CBZombieBlowaway_Click(object sender, RoutedEventArgs e) { if (zombie != null) zombie.Blowaway = (CBZombieBlowaway.IsChecked == true); }
        private void CBZombieDying_Click(object sender, RoutedEventArgs e) { if (zombie != null) zombie.Dying = (CBZombieDying.IsChecked == true); }
        private void CBZombieGarlicBited_Click(object sender, RoutedEventArgs e) { if (zombie != null) zombie.GarlicBited = (CBZombieGarlicBited.IsChecked == true); }
        private void CBZombieExist_Click(object sender, RoutedEventArgs e) { if (zombie != null) zombie.Exist = (CBZombieExist.IsChecked == true); }
        private void SZombieDecelerate_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SZombieDecelerate.IsMouseOver && zombie != null) zombie.DecelerateCountdown = (int)(SZombieDecelerate.Value * 100); }
        private void SZombieFixed_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SZombieFixed.IsMouseOver && zombie != null) zombie.FixedCountdown = (int)(SZombieFixed.Value * 100); }
        private void SZombieFrozen_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SZombieFrozen.IsMouseOver && zombie != null) zombie.FrozenCountdown = (int)(SZombieFrozen.Value * 100); }
        private void BtnZombieButter_Click(object sender, RoutedEventArgs e) { if (zombie != null) zombie.Butter(); }
        private void BtnZombieBlast_Click(object sender, RoutedEventArgs e) { if (zombie != null) zombie.Blast(); }
        private void BtnZombieHit_Click(object sender, RoutedEventArgs e) { if (zombie != null) { int dt = CBZombieDamageType.SelectedIndex; if (dt >= 3) dt++; zombie.Hit(Convert.ToInt32(TBZombieDamage.Text), (PVZ.Zombie.DamageType)dt); } }
        #endregion

        private void CBHpTrack_Click(object sender, RoutedEventArgs e)
        {
            if (CBHpTrack.IsChecked == true) { tracker.IsHide = false; tracker.Show(); }
            else { tracker.Visibility = Visibility.Collapsed; tracker.IsHide = true; }
        }

        private void TBColor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try { tracker.hpfontcolor = (Color)ColorConverter.ConvertFromString(TBColor.Text); } catch { MessageBox.Show("无法转换指定颜色", "错误", MessageBoxButton.OK, MessageBoxImage.Error); }
                TBColor.Foreground = new SolidColorBrush(tracker.hpfontcolor);
            }
        }

        #region Plant
        private void LBPlants_SelectionChanged(object sender, SelectionChangedEventArgs e) { try { preselplantid = Convert.ToInt32(((TextBlock)LBPlants.SelectedItem).Text); } catch { } }
        private void TBPlantX_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && plant != null) plant.X = Convert.ToInt32(TBPlantX.Text); }
        private void TBPlantY_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && plant != null) plant.Y = Convert.ToInt32(TBPlantY.Text); }
        private void TBPlantRow_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && plant != null) plant.Row = Convert.ToInt32(TBPlantRow.Text); }
        private void TBPlantColumn_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && plant != null) plant.Column = Convert.ToInt32(TBPlantColumn.Text); }
        private void TBPlantState_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && plant != null) plant.State = Convert.ToInt32(TBPlantState.Text); }
        private void TBPlantHp_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && plant != null) { plant.Hp = Convert.ToInt32(TBPlantHp.Text); if (plant.Hp > plant.MaxHp) plant.MaxHp = plant.Hp; } }
        private void CBPlantVisible_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Visible = CBPlantVisible.IsChecked != true; }
        private void CBPlantAggressive_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Aggressive = (CBPlantAggressive.IsChecked == true); }
        private void CBPlantSquash_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Squash = (CBPlantSquash.IsChecked == true); }
        private void CBPlantSleeping_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Sleeping = (CBPlantSleeping.IsChecked == true); }
        private void CBPlantExist_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Exist = (CBPlantExist.IsChecked == true); }
        private void SPlantProduct_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SPlantProduct.IsMouseOver && plant != null) plant.ShootOrProductCountdown = (int)(SPlantProduct.Value * 100); }
        private void SPlantAttribute_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SPlantAttribute.IsMouseOver && plant != null) plant.AttributeCountdown = (int)(SPlantProduct.Value * 100); }
        private void SPlantShooting_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SPlantShooting.IsMouseOver && plant != null) plant.ShootingCountdown = (int)(SPlantShooting.Value * 100); }
        private void TBPlantProductInterval_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && plant != null) plant.ShootOrProductInterval = Convert.ToInt32(TBPlantProductInterval.Text); }
        private void BtnPlantEffect_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.CreateEffect(); }
        private void BtnPlantFix_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Fix(); }
        private void BtnPlantFlash_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Flash(); }
        private void BtnPlantLight_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Light(Convert.ToInt32(TBPlantLight.Text)); }
        private void BtnPlantShoot_Click(object sender, RoutedEventArgs e) { if (plant != null) plant.Shoot(Convert.ToInt32(TBPlantShoot.Text)); }
        #endregion

        #region Coin
        private void LBCoins_SelectionChanged(object sender, SelectionChangedEventArgs e) { try { preselcoinid = Convert.ToInt32(((TextBlock)LBCoins.SelectedItem).Text); } catch { } }
        private void TBCoinX_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && coin != null) coin.X = Convert.ToInt32(TBCoinX.Text); }
        private void TBCoinY_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && coin != null) coin.Y = Convert.ToInt32(TBCoinY.Text); }
        private void TBCoinSize_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && coin != null) coin.Size = Convert.ToInt32(TBCoinSize.Text); }
        private void CBCoinCard_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (coin != null && CBCoinCard.IsMouseOver && CBCoinCard.SelectedIndex != 53) coin.CardType = (PVZ.CardType)CBCoinCard.SelectedIndex; }
        private void CBCoinVisible_Click(object sender, RoutedEventArgs e) { if (coin != null) coin.Visible = CBCoinVisible.IsChecked != true; }
        private void CBCoinCollected_Click(object sender, RoutedEventArgs e) { if (coin != null) coin.Collected = (CBCoinCollected.IsChecked == true); }
        private void CBCoinHalo_Click(object sender, RoutedEventArgs e) { if (coin != null) coin.Halo = (CBCoinHalo.IsChecked == true); }
        #endregion

        #region Griditem
        private void LBGriditems_SelectionChanged(object sender, SelectionChangedEventArgs e) { try { preselgriditemid = Convert.ToInt32(((TextBlock)LBGriditems.SelectedItem).Text); } catch { } }
        private void TBGriditemRow_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && griditem != null) griditem.Row = Convert.ToInt32(TBGriditemRow.Text); }
        private void TBGriditemColumn_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && griditem != null) griditem.Column = Convert.ToInt32(TBGriditemColumn.Text); }
        private void CBGriditemExist_Click(object sender, RoutedEventArgs e) { if (griditem != null) griditem.Exist = (CBGriditemExist.IsChecked == true); }
        private void SCraterDisappear_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SCraterDisappear.IsMouseOver && crater != null) crater.DisappearCountdown = (int)(SCraterDisappear.Value * 100); }
        private void CBVaseSkin_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (vase != null) vase.Skin = (PVZ.VaseSkin)(CBVaseSkin.SelectedIndex + 3); }
        private void CBVaseContent_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (vase != null) vase.Content = (PVZ.VaseContent)CBVaseContent.SelectedIndex; }
        private void CBVaseZombie_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (vase != null) vase.Zombie = (PVZ.ZombieType)CBVaseZombie.SelectedIndex; }
        private void CBVasePlant_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (vase != null) vase.Plant = (PVZ.PlantType)CBVasePlant.SelectedIndex; }
        private void TBVaseSun_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && vase != null) vase.Sun = Convert.ToInt32(TBVaseSun.Text); }
        private void SVaseTransparent_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SVaseTransparent.IsMouseOver && crater != null) vase.TransparentCountDown = (int)(SVaseTransparent.Value * 100); }
        #endregion

        #region CardSlot
        private void CBCardSlotVisible_Click(object sender, RoutedEventArgs e) { PVZ.CardSlot.Visible = CBCardSlotVisible.IsChecked; }
        private void LBCards_SelectionChanged(object sender, SelectionChangedEventArgs e) { try { preselcardid = Convert.ToInt32(((TextBlock)LBCards.SelectedItem).Text); } catch { } }
        private void CBCardNum_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBCardNum.IsMouseCaptured) PVZ.CardSlot.SetCardNum(CBCardNum.SelectedIndex); }
        private void TBCardX_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && card != null) card.X = Convert.ToInt32(TBCardX.Text); }
        private void TBCardY_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && card != null) card.Y = Convert.ToInt32(TBCardY.Text); }
        private void CBCardVisible_Click(object sender, RoutedEventArgs e) { if (card != null) card.Visible = CBCardVisible.IsChecked == true; }
        private void CBCardEnable_Click(object sender, RoutedEventArgs e) { if (card != null) card.Enable = (CBCardEnable.IsChecked == true); }
        private void CBCardActive_Click(object sender, RoutedEventArgs e) { if (card != null) card.Active = (CBCardActive.IsChecked == true); }
        private void CBCardType_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBCardType.IsMouseOver && card != null && CBCardType.SelectedIndex != 53) card.CardType = (PVZ.CardType)CBCardType.SelectedIndex; }
        private void CBCardTypeImitative_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBCardTypeImitative.IsMouseOver && card != null && CBCardType.SelectedIndex != 53) card.ImitativeCardType = (PVZ.PlantType)CBCardTypeImitative.SelectedIndex; }
        private void SCardCoolDowm_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SCardCoolDowm.IsMouseOver && card != null) card.CoolDown = (int)(SCardCoolDowm.Value * 100); }
        private void SCardBeltX_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SCardBeltX.IsMouseOver && card != null) card.ConveyorBeltX = (int)(int)SCardBeltX.Value; }
        private void TBCoolDown_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e) && card != null) card.CoolDownInterval = Convert.ToInt32(TBCoolDown.Text); }
        #endregion

        #region Misc
        private void CBUpgradedRepeater_Click(object sender, RoutedEventArgs e) { PVZ.Miscellaneous.UpgradedRepeater = CBUpgradedRepeater.IsChecked == true; }
        private void CBUpgradedFumeshroon_Click(object sender, RoutedEventArgs e) { PVZ.Miscellaneous.UpgradedFumeshroon = CBUpgradedFumeshroon.IsChecked == true; }
        private void CBUpgradedTallnut_Click(object sender, RoutedEventArgs e) { PVZ.Miscellaneous.UpgradedTallnut = CBUpgradedTallnut.IsChecked == true; }
        private void SAttributeTime_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { PVZ.Miscellaneous.AttributeCountdown = (int)(SAttributeTime.Value * 100); }
        private void TBLevelProcess_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Miscellaneous.LevelProcess = Convert.ToInt32(TBLevelProcess.Text); }
        private void TBLevelRound_PreviewKeyDown(object sender, KeyEventArgs e) { if (DealKeyDown(sender, e)) PVZ.Miscellaneous.Round = Convert.ToInt32(TBLevelRound.Text); }
        #endregion

        #region Music
        private void CBMusicType_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBMusicType.IsMouseCaptured) PVZ.Music.Type = (PVZ.MusicType)(CBMusicType.SelectedIndex + 1); }
        private void CBInGameEnanle_Click(object sender, RoutedEventArgs e) { PVZ.Music.INGAMEEnable = CBINGAMEEnable.IsChecked == true; }
        private void CBInGameStart_Click(object sender, RoutedEventArgs e) { PVZ.Music.INGAMEStart = CBINGAMEStart.IsChecked == true; }
        private void SMusicSpeed_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SMusicSpeed.IsMouseOver) PVZ.Bass_Dll.MusicSetAttribute(PVZ.Bass_Dll.HMUSIC1, PVZ.Bass_Dll.BASS_MUSIC_ATTRIB_SPEED, (short)(int)SMusicSpeed.Value); }
        private void SMusicVolumn_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SMusicVolumn.IsMouseOver) PVZ.Bass_Dll.MusicSetAttribute(PVZ.Bass_Dll.HMUSIC1, PVZ.Bass_Dll.BASS_MUSIC_ATTRIB_AMPLIFY, (short)(int)SMusicVolumn.Value); }
        private void SMusicBPM_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { if (SMusicBPM.IsMouseOver) PVZ.Bass_Dll.MusicSetAttribute(PVZ.Bass_Dll.HMUSIC1, PVZ.Bass_Dll.BASS_MUSIC_ATTRIB_BPM, (short)(int)SMusicBPM.Value); }
        #endregion

        private void TCMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5) { var selItem = (TabItem)TCMain.SelectedItem; selItem.Tag = TCMain; TCMain.Items.Remove(selItem); var separate = new SeparateWindow(); separate.TCMain.Items.Add(selItem); separate.Show(); }
        }

        public int scale = 100;

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Delta > 0) scale += 5; else scale -= 5;
                scale = Math.Max(10, scale); scale = Math.Min(300, scale);
                UIElement con = Content as UIElement;
                con.RenderTransform = new ScaleTransform(scale / 100.0, scale / 100.0);
                Height = 470.0 * scale / 100; Width = 400.0 * scale / 100;
            }
        }

        private void CBSceneType_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CBSceneType.IsMouseCaptured) PVZ.Scene = (PVZ.SceneType)CBSceneType.SelectedIndex; }
    }
}
