using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Damareen
{
    public partial class NewWorld : Window
    {
        private Kartya editingCard = null;
        private Kazamata editingDungeon = null;
        private MediaPlayer backgroundMusicPlayer;

        public NewWorld(MediaPlayer player = null)
        {
            InitializeComponent();

            if (player != null)
            {
                this.backgroundMusicPlayer = player;
            }
            else
            {
                this.backgroundMusicPlayer = new MediaPlayer();
                StartBackgroundMusic();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
            RefreshDungeonList();
            PopulateBonusCardCombo();
            PopulateBaseCardCombo();

            this.Opacity = 0;
            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.8));
            this.BeginAnimation(OpacityProperty, fade);
        }

        private void StartBackgroundMusic()
        {
            try
            {
                string musicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", "music.mp3");

                if (File.Exists(musicPath))
                {
                    backgroundMusicPlayer.Open(new Uri(musicPath));
                    backgroundMusicPlayer.Volume = 0.2;

                    backgroundMusicPlayer.MediaEnded += (sender, e) =>
                    {
                        backgroundMusicPlayer.Position = TimeSpan.Zero;
                        backgroundMusicPlayer.Play();
                    };

                    backgroundMusicPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Zene hiba: " + ex.Message);
            }
        }

        private void CreateOrUpdateCard(object sender, RoutedEventArgs e)
        {
            try
            {
                string nev = InputName.Text;
                if (string.IsNullOrWhiteSpace(nev)) throw new Exception("Adj nevet a kártyának!");
                if (nev.Length > 16) throw new Exception("A név túl hosszú (max 16)!");

                if (!int.TryParse(InputHp.Text, out int hp)) throw new Exception("Hibás Életerő!");
                if (hp < 1 || hp > 100) throw new Exception("Az életerő (1-100) tartományon kívül van!");

                if (!int.TryParse(InputDmg.Text, out int dmg)) throw new Exception("Hibás Sebzés!");
                if (dmg < 2 || dmg > 100) throw new Exception("A sebzés (2-100) tartományon kívül van!");

                string type = ((ComboBoxItem)TypeCombo.SelectedItem).Content.ToString().ToLower();
                switch (type)
                {
                    case "tűz": type = "tuz"; break;
                    case "víz": type = "viz"; break;
                    case "föld": type = "fold"; break;
                    case "levegő": type = "levego"; break;
                }

                if (editingCard != null)
                {
                    if (!editingCard.SimaKartya)
                    {
                        throw new Exception("Vezérkártyák ebben a szerkesztőben nem módosíthatók.");
                    }

                    editingCard.Nev = nev;
                    editingCard.Hp = hp;
                    editingCard.CurrentHp = hp;
                    editingCard.Attack = dmg;
                    editingCard.SimaKartya = true;

                    editingCard.a = new string[] { "uj kartya", nev, dmg.ToString(), hp.ToString(), type };

                    switch (type)
                    {
                        case "tuz": editingCard.Kartyatipus = KartyaTipusok.Fire; break;
                        case "viz": editingCard.Kartyatipus = KartyaTipusok.Water; break;
                        case "fold": editingCard.Kartyatipus = KartyaTipusok.Earth; break;
                        case "levego": editingCard.Kartyatipus = KartyaTipusok.Air; break;
                    }

                    Kartya gyujtemenyKartya = Jatekos.gyujtemeny.FirstOrDefault(gk => gk.Nev == editingCard.Nev);
                    if (gyujtemenyKartya != null)
                    {
                        gyujtemenyKartya.Attack = dmg;
                        gyujtemenyKartya.Hp = hp;
                        gyujtemenyKartya.CurrentHp = hp;
                        gyujtemenyKartya.Kartyatipus = editingCard.Kartyatipus;
                        gyujtemenyKartya.a = editingCard.a;
                        gyujtemenyKartya.UpdateArrayData();
                    }
                }
                else
                {
                    if (Vilag.kartyak.Any(k => k.Nev.Equals(nev, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new Exception("Egy kártya már létezik ezen a néven!");
                    }

                    string command = "uj kartya";
                    string[] data = new string[] { command, nev, dmg.ToString(), hp.ToString(), type };
                    new Kartya(data, true);
                }

                ClearInputs();
                RefreshList();
                PopulateBonusCardCombo();
                PopulateBaseCardCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateLeader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nev = InputLeaderName.Text.Trim();
                if (string.IsNullOrWhiteSpace(nev)) throw new Exception("Adj nevet a vezérnek!");
                if (nev.Length > 16) throw new Exception("A vezér neve túl hosszú (max 16)!");

                if (Vilag.kartyak.Any(k => k.Nev.Equals(nev, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception("Egy kártya (sima vagy vezér) már létezik ezen a néven!");
                }

                if (ComboBaseCard.SelectedItem == null)
                {
                    throw new Exception("Válassz egy alap kártyát a vezérnek!");
                }
                string alapKartyaNev = (ComboBaseCard.SelectedItem as ComboBoxItem).Content.ToString();

                string bonuszTipus = (RadioLeaderBonusHp.IsChecked == true) ? "eletero" : "sebzes";

                string[] data = new string[] { "uj vezer", nev, alapKartyaNev, bonuszTipus };
                new Kartya(data, true);

                InputLeaderName.Text = "";
                ComboBaseCard.SelectedIndex = -1;
                RadioLeaderBonusHp.IsChecked = true;

                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshList()
        {
            if (CardsPanel == null) return;

            CardsPanel.Children.Clear();
            bool showLeader = FilterLeader.IsChecked == true;
            bool showNormal = FilterNormal.IsChecked == true;

            List<Kartya> kartyakCopy = new List<Kartya>(Vilag.kartyak);

            foreach (Kartya k in kartyakCopy)
            {
                bool isLeader = !k.SimaKartya;

                if ((isLeader && showLeader) || (!isLeader && showNormal))
                {
                    MiniCard mc = new MiniCard(k);
                    mc.Margin = new Thickness(10);
                    mc.Width = 170;
                    mc.Height = 240;
                    mc.Cursor = Cursors.Hand;

                    ContextMenu menu = new ContextMenu();

                    MenuItem editItem = new MenuItem { Header = "Szerkesztés" };
                    editItem.Click += (s, args) => LoadCardForEdit(k);
                    menu.Items.Add(editItem);

                    MenuItem deleteItem = new MenuItem { Header = "Törlés" };
                    deleteItem.Click += (s, args) =>
                    {
                        if (MessageBox.Show($"Biztosan törölni szeretnéd: {k.Nev}?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Vilag.kartyak.Remove(k);

                            Kartya gyujtemenyKartya = Jatekos.gyujtemeny.FirstOrDefault(gk => gk.Nev == k.Nev);
                            if (gyujtemenyKartya != null)
                            {
                                Jatekos.gyujtemeny.Remove(gyujtemenyKartya);
                            }

                            Kartya pakliKartya = Jatekos.pakli.FirstOrDefault(pk => pk.Nev == k.Nev);
                            if (pakliKartya != null)
                            {
                                Jatekos.pakli.Remove(pakliKartya);
                            }

                            if (editingCard == k) ClearInputs();
                            RefreshList();
                            PopulateBonusCardCombo();
                            PopulateBaseCardCombo();
                        }
                    };
                    menu.Items.Add(deleteItem);

                    mc.ContextMenu = menu;

                    mc.MouseLeftButtonUp += (s, args) =>
                    {
                        if (mc.ContextMenu != null)
                        {
                            mc.ContextMenu.IsOpen = true;
                        }
                    };

                    CardsPanel.Children.Add(mc);
                }
            }
        }

        private void LoadCardForEdit(Kartya k)
        {
            if (!k.SimaKartya)
            {
                MessageBox.Show($"A vezérkártyák ('{k.Nev}') nem szerkeszthetők.\nA vezérek a világfájlban (in.txt) definiálhatók sima kártyák alapján.", "Nem Szerkeszthető", MessageBoxButton.OK, MessageBoxImage.Warning);
                ClearInputs();
                return;
            }

            Kartya gyujtemenyKartya = Jatekos.gyujtemeny.FirstOrDefault(gk => gk.Nev == k.Nev);
            if (gyujtemenyKartya != null)
            {
                editingCard = gyujtemenyKartya;
                InputName.Text = gyujtemenyKartya.Nev;
                InputHp.Text = gyujtemenyKartya.Hp.ToString();
                InputDmg.Text = gyujtemenyKartya.Attack.ToString();

                switch (gyujtemenyKartya.Kartyatipus)
                {
                    case KartyaTipusok.Fire: TypeCombo.SelectedIndex = 0; break;
                    case KartyaTipusok.Water: TypeCombo.SelectedIndex = 1; break;
                    case KartyaTipusok.Earth: TypeCombo.SelectedIndex = 2; break;
                    case KartyaTipusok.Air: TypeCombo.SelectedIndex = 3; break;
                }
            }
            else
            {
                editingCard = k;
                InputName.Text = k.Nev;
                InputHp.Text = k.Hp.ToString();
                InputDmg.Text = k.Attack.ToString();

                switch (k.Kartyatipus)
                {
                    case KartyaTipusok.Fire: TypeCombo.SelectedIndex = 0; break;
                    case KartyaTipusok.Water: TypeCombo.SelectedIndex = 1; break;
                    case KartyaTipusok.Earth: TypeCombo.SelectedIndex = 2; break;
                    case KartyaTipusok.Air: TypeCombo.SelectedIndex = 3; break;
                }
            }

            CreateBtn.Content = "MÓDOSÍTÁS";
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ClearInputs()
        {
            editingCard = null;
            InputName.Text = "";
            InputHp.Text = "";
            InputDmg.Text = "";
            TypeCombo.SelectedIndex = 0;
            CreateBtn.Content = "LÉTREHOZÁS";
        }

        private bool ValidateAllDungeons()
        {
            List<string> incompleteKazamatak = new List<string>();

            foreach (Kazamata k in Vilag.kazamatak)
            {
                if (!k.IsComplete())
                {
                    (int maxSima, int maxVezer) = k.GetKazamataLimits();
                    int currentSima = k.KKartyak.Count(card => card.SimaKartya);
                    int currentVezer = k.KKartyak.Count(card => !card.SimaKartya);

                    string hianyzoSima = currentSima < maxSima ? $"{maxSima - currentSima} sima kártya" : "";
                    string hianyzoVezer = currentVezer < maxVezer ? $"{maxVezer - currentVezer} vezér" : "";

                    string hianyzo = string.Join(" és ", new[] { hianyzoSima, hianyzoVezer }.Where(s => !string.IsNullOrEmpty(s)));

                    incompleteKazamatak.Add($"• {k.KNev} ({k.KTipus}): Hiányzik {hianyzo}");
                }
            }

            if (incompleteKazamatak.Count > 0)
            {
                string message = "A következő kazamatákból még hiányoznak kártyák:\n\n" + string.Join("\n", incompleteKazamatak);
                MessageBox.Show(message, "Hiányos Kazamaták", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BackToMenu(object sender, RoutedEventArgs e)
        {
            if (!ValidateAllDungeons())
            {
                return;
            }

            MainWindow mw = new MainWindow(backgroundMusicPlayer);
            mw.Show();
            this.Close();
        }

        private void SwitchToCards(object sender, RoutedEventArgs e)
        {
            CardCreationPanel.Visibility = Visibility.Visible;
            DungeonCreationPanel.Visibility = Visibility.Collapsed;
            BtnShowCards.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5A059"));
            BtnShowDungeons.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888"));
        }

        private void SwitchToDungeons(object sender, RoutedEventArgs e)
        {
            CardCreationPanel.Visibility = Visibility.Collapsed;
            DungeonCreationPanel.Visibility = Visibility.Visible;
            BtnShowDungeons.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5A059"));
            BtnShowCards.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888"));
            RefreshDungeonList();
        }

        private void SaveWorld_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAllDungeons())
            {
                return;
            }

            try
            {
                Vilag.ExportVilag(Vilag.VilagFajlNev);
                MessageBox.Show($"Világ elmentve: {Vilag.VilagFajlNev}", "Mentés Sikeres", MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow mw = new MainWindow(backgroundMusicPlayer);
                mw.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a mentés során: {ex.Message}", "Mentési Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshDungeonList()
        {
            DungeonsPanel.Children.Clear();
            foreach (Kazamata k in Vilag.kazamatak)
            {
                Button b = new Button
                {
                    Content = k.KNev,
                    Tag = k,
                    Style = (Style)Application.Current.Resources["EditorListButton"]
                };
                if (k == editingDungeon)
                {
                    b.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a1a1a"));
                    b.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5A059"));
                }
                b.Click += LoadDungeonForEdit;

                ContextMenu menu = new ContextMenu();
                MenuItem deleteItem = new MenuItem { Header = "Törlés" };
                deleteItem.Click += (s, args) =>
                {
                    if (MessageBox.Show($"Biztosan törölni szeretnéd: {k.KNev}?", "Törlés", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Vilag.kazamatak.Remove(k);
                        if (editingDungeon == k) ClearDungeonInputs();
                        RefreshDungeonList();
                    }
                };
                menu.Items.Add(deleteItem);
                b.ContextMenu = menu;

                DungeonsPanel.Children.Add(b);
            }
        }

        private void LoadDungeonForEdit(object sender, RoutedEventArgs e)
        {
            Kazamata k = (sender as Button).Tag as Kazamata;
            editingDungeon = k;

            InputDungeonName.Text = k.KNev;
            switch (k.KTipus)
            {
                case KTipusok.Egyszeru: DungeonTypeCombo.SelectedIndex = 0; break;
                case KTipusok.Kis: DungeonTypeCombo.SelectedIndex = 1; break;
                case KTipusok.Nagy: DungeonTypeCombo.SelectedIndex = 2; break;
            }

            switch (k.bonusz)
            {
                case Bonuszok.Eletero:
                    RadioBonusHp.IsChecked = true;
                    InputBonusValue.Text = k.BonuszErtek.ToString();
                    break;
                case Bonuszok.Sebzes:
                    RadioBonusDmg.IsChecked = true;
                    InputBonusValue.Text = k.BonuszErtek.ToString();
                    break;
                case Bonuszok.UjKartya:
                    RadioBonusCard.IsChecked = true;
                    ComboBonusCard.SelectedValue = k.BonuszKartyaNev;
                    break;
            }

            BonusType_Changed(null, null);
            CreateDungeonBtn.Content = "MÓDOSÍTÁS";

            RefreshDungeonList();
            RefreshDungeonCardLists();
        }

        private void CreateOrUpdateDungeon(object sender, RoutedEventArgs e)
        {
            try
            {
                string nev = InputDungeonName.Text;
                if (string.IsNullOrWhiteSpace(nev)) throw new Exception("Adj nevet a kazamatának!");
                if (nev.Length > 20) throw new Exception("A kazamata neve túl hosszú (max 20 karakter)!");

                if (editingDungeon == null && Vilag.kazamatak.Any(k => k.KNev.Equals(nev, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception("Egy kazamata már létezik ezen a néven!");
                }

                KTipusok tipus = KTipusok.Egyszeru;
                switch (DungeonTypeCombo.SelectedIndex)
                {
                    case 1: tipus = KTipusok.Kis; break;
                    case 2: tipus = KTipusok.Nagy; break;
                }

                if (editingDungeon != null)
                {
                    editingDungeon.KNev = nev;
                    editingDungeon.KTipus = tipus;
                }
                else
                {
                    editingDungeon = new Kazamata(nev, tipus);
                }

                if (tipus == KTipusok.Nagy)
                {
                    editingDungeon.bonusz = Bonuszok.UjKartya;
                    editingDungeon.BonuszKartyaNev = "";
                }
                else if (RadioBonusHp.IsChecked == true)
                {
                    editingDungeon.bonusz = Bonuszok.Eletero;
                    editingDungeon.BonuszErtek = 2;
                }
                else if (RadioBonusDmg.IsChecked == true)
                {
                    editingDungeon.bonusz = Bonuszok.Sebzes;
                    editingDungeon.BonuszErtek = 1;
                }
                else
                {
                    throw new Exception("Ismeretlen bónusz típus 'egyszeru'/'kis' kazamatához.");
                }

                ClearDungeonInputs();
                RefreshDungeonList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearDungeonInputs()
        {
            editingDungeon = null;
            InputDungeonName.Text = "";
            DungeonTypeCombo.SelectedIndex = 0;
            RadioBonusHp.IsChecked = true;
            InputBonusValue.Text = "";
            ComboBonusCard.SelectedIndex = -1;

            CreateDungeonBtn.Content = "LÉTREHOZÁS";

            RefreshDungeonCardLists();
            RefreshDungeonList();
            UpdateDungeonContentCounter();
        }

        private void BonusType_Changed(object sender, RoutedEventArgs e)
        {
            if (BonusValuePanel == null || BonusCardPanel == null) return;

            bool isNagy = DungeonTypeCombo.SelectedIndex == 2;
            bool isEgyszeruVagyKis = DungeonTypeCombo.SelectedIndex == 0 || DungeonTypeCombo.SelectedIndex == 1;

            RadioBonusCard.IsEnabled = !isEgyszeruVagyKis;
            RadioBonusHp.IsEnabled = !isNagy;
            RadioBonusDmg.IsEnabled = !isNagy;

            if (isNagy)
            {
                BonusValuePanel.Visibility = Visibility.Collapsed;
                BonusCardPanel.Visibility = Visibility.Collapsed;
                RadioBonusCard.IsChecked = true;
            }
            else if (isEgyszeruVagyKis)
            {
                BonusCardPanel.Visibility = Visibility.Collapsed;
                BonusValuePanel.Visibility = Visibility.Visible;
                if (RadioBonusCard.IsChecked == true)
                {
                    RadioBonusHp.IsChecked = true;
                }
            }
        }

        private void PopulateBonusCardCombo()
        {
            ComboBonusCard.Items.Clear();
            foreach (Kartya k in Vilag.kartyak.Where(k => k.SimaKartya))
            {
                ComboBoxItem item = new ComboBoxItem { Content = k.Nev };
                ComboBonusCard.Items.Add(item);
            }
        }

        private void PopulateBaseCardCombo()
        {
            ComboBaseCard.Items.Clear();
            foreach (Kartya k in Vilag.kartyak.Where(k => k.SimaKartya))
            {
                ComboBoxItem item = new ComboBoxItem { Content = k.Nev };
                ComboBaseCard.Items.Add(item);
            }
        }

        private (int maxSima, int maxVezer) GetDungeonLimits(KTipusok tipus)
        {
            switch (tipus)
            {
                case KTipusok.Egyszeru: return (1, 0);
                case KTipusok.Kis: return (3, 1);
                case KTipusok.Nagy: return (5, 1);
                default: return (0, 0);
            }
        }

        private void UpdateDungeonContentCounter()
        {
            if (editingDungeon == null)
            {
                DungeonContentCountText.Text = "(Sima: 0/0, Vezér: 0/0)";
                return;
            }

            (int maxSima, int maxVezer) = GetDungeonLimits(editingDungeon.KTipus);
            int currentSima = editingDungeon.KKartyak.Count(k => k.SimaKartya);
            int currentVezer = editingDungeon.KKartyak.Count(k => !k.SimaKartya);

            DungeonContentCountText.Text = $"(Sima: {currentSima}/{maxSima}, Vezér: {currentVezer}/{maxVezer})";
        }

        private void RefreshDungeonCardLists()
        {
            if (AvailableCardsPanel == null || DungeonCardsPanel == null)
            {
                return;
            }
            AvailableCardsPanel.Children.Clear();
            DungeonCardsPanel.Children.Clear();

            if (editingDungeon == null)
            {
                AvailableContentCountText.Text = $"({Vilag.kartyak.Count})";
                UpdateDungeonContentCounter();
                return;
            }

            foreach (Kartya k in Vilag.kartyak)
            {
                MiniCard mc = new MiniCard(k);
                mc.Width = 170;
                mc.Height = 240;
                mc.Cursor = Cursors.Hand;

                if (editingDungeon.KKartyak.Any(dk => dk.Nev == k.Nev))
                {
                    mc.Mode = MiniCardMode.DungeonEditor_InDungeon;
                    DungeonCardsPanel.Children.Add(mc);
                }
                else
                {
                    mc.Mode = MiniCardMode.DungeonEditor_Available;
                    AvailableCardsPanel.Children.Add(mc);
                }
            }

            AvailableContentCountText.Text = $"({AvailableCardsPanel.Children.Count})";
            UpdateDungeonContentCounter();
        }

        public void HandleDungeonCardClick(Kartya kartya, MiniCard miniCard, MiniCardMode mode)
        {
            if (editingDungeon == null)
            {
                MessageBox.Show("Nincs kiválasztva kazamata a szerkesztéshez.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (mode == MiniCardMode.DungeonEditor_Available)
            {
                (int maxSima, int maxVezer) = GetDungeonLimits(editingDungeon.KTipus);
                int currentSima = editingDungeon.KKartyak.Count(k => k.SimaKartya);
                int currentVezer = editingDungeon.KKartyak.Count(k => !k.SimaKartya);

                if (kartya.SimaKartya)
                {
                    if (currentSima >= maxSima)
                    {
                        MessageBox.Show($"Ennek a kazamatának ({editingDungeon.KTipus}) legfeljebb {maxSima} sima kártyája lehet.", "Limit Elérve", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    if (currentVezer >= maxVezer)
                    {
                        MessageBox.Show($"Ennek a kazamatának ({editingDungeon.KTipus}) legfeljebb {maxVezer} vezér kártyája lehet.", "Limit Elérve", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                editingDungeon.KKartyak.Add(kartya.Clone());
            }
            else if (mode == MiniCardMode.DungeonEditor_InDungeon)
            {
                Kazamata k = editingDungeon;
                Kartya kartyaToRemove = k.KKartyak.FirstOrDefault(c => c.Nev == kartya.Nev);
                if (kartyaToRemove != null)
                {
                    k.KKartyak.Remove(kartyaToRemove);
                }
            }

            RefreshDungeonCardLists();
        }

        private void DungeonType_Changed(object sender, SelectionChangedEventArgs e)
        {
            BonusType_Changed(sender, null);
            RefreshDungeonCardLists();
        }
    }
}