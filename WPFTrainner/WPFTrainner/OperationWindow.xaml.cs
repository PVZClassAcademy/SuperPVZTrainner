using System;
using System.Windows;
using System.Windows.Input;
using ITrainerExtension;
using PVZClass;

namespace PVZWPFTrainner
{
    public partial class OperationWindow : Window
    {
        public OperationWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Lang.ChangeLanguage(Content);
        }

        public int scale = 100;
        private Random random = new Random();

        private void Window_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (e.Delta > 0) scale += 5; else scale -= 5;
                scale = Math.Max(10, scale);
                scale = Math.Min(300, scale);
                System.Windows.UIElement con = Content as System.Windows.UIElement;
                con.RenderTransform = new System.Windows.Media.ScaleTransform(scale / 100.0, scale / 100.0);
                Height = 670.0 * scale / 100;
                Width = 600.0 * scale / 100;
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (System.InvalidOperationException) { }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow != null)
                ((MainWindow)Application.Current.MainWindow).BtnOperate.IsEnabled = true;
        }

        private void BtnResumeLawnmover_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PVZ.ResumeLawnmover();
        }

        private void BtnStartLawnmover_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PVZ.StartLawnmover();
        }

        private void CreateBatch(Action<int, int> action)
        {
            int row = (int)NudRow.Value;
            int column = (int)NudColumn.Value;
            if (row == 0 && column > 0)
            {
                for (int index = 0; index <= PVZ.RouteCount - 1; index++)
                    action(index, column - 1);
            }
            else if (column == 0 && row > 0)
            {
                for (int jndex = 0; jndex <= 8; jndex++)
                    action(row - 1, jndex);
            }
            else if (column == 0 && row == 0)
            {
                for (int index = 0; index <= PVZ.RouteCount - 1; index++)
                    for (int jndex = 0; jndex <= 8; jndex++)
                        action(index, jndex);
            }
            else
            {
                action(row - 1, column - 1);
            }
        }

        private void BtnCreatePlant_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateBatch(CreateSinglePlant);
        }

        private void CreateSinglePlant(int row, int column)
        {
            PVZ.Plant plant = PVZ.CreatePlant((PVZ.PlantType)CBPlantTypes.SelectedIndex, (byte)row, (byte)column, CBPlantIsImitate.IsChecked.Value);
            int hp = Convert.ToInt32(Convert.ToDouble(TBPlantHp.Text));
            if (hp > 0) { plant.MaxHp = hp; plant.Hp = hp; }
            if (CBPlantIsFixed.IsChecked == true) plant.Fix();
        }

        private void BtnCreateZombie_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateBatch(CreateSingleZombie);
        }

        private void CreateSingleZombie(int row, int column)
        {
            PVZ.Zombie zombie = PVZ.CreateZombie((PVZ.ZombieType)CBZombieTypes.SelectedIndex, (byte)row, (byte)column);
            int hp = Convert.ToInt32(Convert.ToDouble(TBZombieBodyHp.Text));
            if (hp > 0) { zombie.MaxBodyHP = hp; zombie.BodyHP = hp; }
            hp = Convert.ToInt32(Convert.ToDouble(TBZombieHatHp.Text));
            if (hp > 0) { zombie.MaxAccessoriesType1HP = hp; zombie.AccessoriesType1HP = hp; }
            hp = Convert.ToInt32(Convert.ToDouble(TBZombieShieldHp.Text));
            if (hp > 0) { zombie.MaxAccessoriesType2HP = hp; zombie.AccessoriesType2HP = hp; }
            if (CBZombieIsHypnotized.IsChecked == true) zombie.Hypnotized = true;
        }

        private void BtnCreateLadder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateBatch((r, c) => PVZ.CreateLadder((byte)r, (byte)c));
        }

        private void BtnCreateGrave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateBatch((r, c) => PVZ.CreateGrave((byte)r, (byte)c));
        }

        private void BtnAutoLadder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var griditem in PVZ.AllGriditems)
            {
                if (griditem.Type == PVZ.GriditemType.Ladder)
                    griditem.Exist = false;
            }
            foreach (var plant in PVZ.AllPlants)
            {
                if (plant.Type == PVZ.PlantType.Pumpkin)
                    PVZ.CreateLadder((byte)plant.Row, (byte)plant.Column);
            }
        }

        private void BtnCreatRake_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateBatch((r, c) => PVZ.CreateRake((byte)r, (byte)c));
        }

        private void BtnCreateCoin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CBCoinGrid.IsChecked == true)
                CreateBatch(CreateGridCoin);
            else if (CBCoinGrid.IsChecked == false)
                PVZ.CreateCoin(GetEnumTypeValue<PVZ.CoinType>(CBCoinTypes.SelectedIndex), (byte)(int)NudColumn.Value, (byte)(int)NudRow.Value, (PVZ.Coin.MotionType)CBCoinMotionTypes.SelectedIndex, (PVZ.CardType)CBCardTypes.SelectedIndex);
        }

        private TEnum GetEnumTypeValue<TEnum>(int value) where TEnum : struct
        {
            return (TEnum)Enum.GetValues(typeof(TEnum)).GetValue(value);
        }

        private void CreateGridCoin(int row, int column)
        {
            PVZ.RCToXY(ref row, ref column);
            PVZ.CreateCoin(GetEnumTypeValue<PVZ.CoinType>(CBCoinTypes.SelectedIndex), (byte)column, (byte)row, (PVZ.Coin.MotionType)CBCoinMotionTypes.SelectedIndex, (PVZ.CardType)CBCardTypes.SelectedIndex);
        }

        private void CreateSingleVase(int row, int column)
        {
            PVZ.VaseContent vaseContent = (PVZ.VaseContent)CBVaseContent.SelectedIndex;
            PVZ.VaseSkin vaseSkin = (PVZ.VaseSkin)(CBVaseSkin.SelectedIndex + 3);
            PVZ.ZombieType zombie = (PVZ.ZombieType)CBVaseZombie.SelectedIndex;
            PVZ.PlantType plant = (PVZ.PlantType)CBVasePlant.SelectedIndex;
            int sun = Convert.ToInt32(Convert.ToDouble(TBVaseSun.Text));
            if (CBVaseRandom.IsChecked == true)
            {
                int per = random.Next(62);
                if (per == 0) vaseContent = PVZ.VaseContent.None;
                else if (per < 30) vaseContent = PVZ.VaseContent.Plant;
                else if (per < 60) vaseContent = PVZ.VaseContent.Zombie;
                else vaseContent = PVZ.VaseContent.Sun;
                vaseSkin = PVZ.VaseSkin.Unknow;
                switch (vaseContent)
                {
                    case PVZ.VaseContent.Zombie:
                        zombie = (PVZ.ZombieType)random.Next(MIVaseRandomExclude.IsChecked == true ? 25 : 33);
                        if (random.Next(40) == 0) vaseSkin = PVZ.VaseSkin.Zombie;
                        break;
                    case PVZ.VaseContent.Plant:
                        plant = (PVZ.PlantType)random.Next(MIVaseRandomExclude.IsChecked == true ? 40 : 53);
                        if (random.Next(40) == 0) vaseSkin = PVZ.VaseSkin.Leaf;
                        break;
                    case PVZ.VaseContent.Sun:
                        sun = random.Next(5);
                        break;
                }
            }
            PVZ.CreateVase(row, column, vaseContent, vaseSkin, zombie, plant, sun);
        }

        private void BtnCreateVase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateBatch(CreateSingleVase);
        }

        private void BtnCreateCaption_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CBCaptionImageData.IsChecked == true)
                PVZ.CreateImageCaption(TBCaptionText.Text);
            else if (CBCaptionImageData.IsChecked == false)
                PVZ.CreateCaption(TBCaptionText.Text, GetEnumTypeValue<PVZ.CaptionStyle>(CBCaptionStyle.SelectedIndex));
        }

        private void BtnCreatePlantEffectType_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PVZ.CreatePlantEffect(GetEnumTypeValue<PVZ.PlantEffectType>(CBPlantEffectType.SelectedIndex), (int)NudColumn.Value, (int)NudRow.Value);
        }

        private void BtnCreateEffect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PVZ.CreateEffect(Convert.ToInt32(Convert.ToDouble(TBEffectType.Text)), (int)NudColumn.Value, (int)NudRow.Value);
        }

        private void BtnCreateSound_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (BtnCreateSound.IsChecked == true)
                PVZ.CreateSound(Convert.ToInt32(Convert.ToDouble(TBSoundType.Text)));
            else
                PVZ.StopSound(Convert.ToInt32(Convert.ToDouble(TBSoundType.Text)));
        }

        private void BtnCreateExplosion_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PVZ.CreateExplosion((int)NudColumn.Value, (int)NudRow.Value, (int)Convert.ToDouble(TBExplosionRadius.Text), CBIsCinder.IsChecked.Value, (byte)(int)NudExplosionBound.Value, CBIsEnemy.IsChecked.Value);
        }
    }
}
