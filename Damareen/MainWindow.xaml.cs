using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Damareen
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer emberTimer;
        private Random rand = new Random();
        private MediaPlayer backgroundMusicPlayer;
        private bool isNavigating = false;
        private string worldCreationMode = "";
        private string defaultCampaignFile = "in.txt";
        private string gameBaseFolder = "GameFiles";
        private string savesFolder = "Saves";
        private string selectedWorldFile = "";
        private bool isMusicPlaying = true;

        public MainWindow()
        {
            InitializeComponent();
            backgroundMusicPlayer = new MediaPlayer();
            Load();
            StartEmberAnimation();
            StartBackgroundMusic();
        }

        public MainWindow(MediaPlayer player)
        {
            InitializeComponent();
            this.backgroundMusicPlayer = player;
            Load();
            StartEmberAnimation();
        }

        private void StartBackgroundMusic()
        {
            try
            {
                string musicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, gameBaseFolder, "music.mp3");

                if (File.Exists(musicPath))
                {
                    backgroundMusicPlayer.Open(new Uri(musicPath));
                    backgroundMusicPlayer.Volume = 0.15;
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
        private void ToggleMusic(object sender, RoutedEventArgs e)
        {
            if (isMusicPlaying)
            {
                backgroundMusicPlayer.Pause();
                MusicToggleBtn.Content = "🔇";
                MusicToggleBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666"));
                isMusicPlaying = false;
            }
            else
            {
                backgroundMusicPlayer.Play();
                MusicToggleBtn.Content = "🔊";
                MusicToggleBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5A059"));
                isMusicPlaying = true;
            }
        }

        private void StartEmberAnimation()
        {
            emberTimer = new DispatcherTimer();
            emberTimer.Interval = TimeSpan.FromMilliseconds(80);
            emberTimer.Tick += (s, e) => SpawnEmber();
            emberTimer.Start();
        }

        private void SpawnEmber()
        {
            double size = rand.Next(4, 10);

            RadialGradientBrush glowBrush = new RadialGradientBrush();
            glowBrush.GradientOrigin = new Point(0.5, 0.5);
            glowBrush.Center = new Point(0.5, 0.5);
            glowBrush.RadiusX = 0.5;
            glowBrush.RadiusY = 0.5;

            byte red = 255;
            byte green = (byte)rand.Next(50, 160);

            glowBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, red, green, 0), 0.0));
            glowBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, red, green, 0), 1.0));

            Ellipse ember = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = glowBrush,
                Opacity = 0,
                RenderTransform = new TranslateTransform()
            };

            double startX = rand.NextDouble() * SystemParameters.PrimaryScreenWidth;
            double startY = SystemParameters.PrimaryScreenHeight + 20;

            Canvas.SetLeft(ember, startX);
            Canvas.SetTop(ember, startY);

            BackgroundCanvas.Children.Add(ember);

            DoubleAnimation moveUp = new DoubleAnimation
            {
                To = -50,
                Duration = TimeSpan.FromSeconds(rand.Next(6, 15))
            };

            DoubleAnimation opacityAnim = new DoubleAnimation
            {
                From = 0,
                To = rand.NextDouble() * 0.8 + 0.2,
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            DoubleAnimation swayAnim = new DoubleAnimation
            {
                To = (rand.NextDouble() - 0.5) * 200,
                Duration = TimeSpan.FromSeconds(rand.Next(3, 7)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            ember.BeginAnimation(Canvas.TopProperty, moveUp);
            ember.BeginAnimation(OpacityProperty, opacityAnim);
            ((TranslateTransform)ember.RenderTransform).BeginAnimation(TranslateTransform.XProperty, swayAnim);

            moveUp.Completed += (s, e) => BackgroundCanvas.Children.Remove(ember);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void ShowGameModeSelect(object sender, RoutedEventArgs e)
        {
            MenuStack.Visibility = Visibility.Collapsed;
            GameModeSelectUI.Visibility = Visibility.Visible;
            GameModeSelectUI.Opacity = 0;
            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            GameModeSelectUI.BeginAnimation(OpacityProperty, fade);
        }

        private void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            GameModeSelectUI.Visibility = Visibility.Collapsed;
            DifficultySelectUI.Visibility = Visibility.Collapsed;
            CustomWorldSelectUI.Visibility = Visibility.Collapsed;
            WorldNameInputUI.Visibility = Visibility.Collapsed;
            WorldLoadUI.Visibility = Visibility.Collapsed;
            WorldCreateOrLoadUI.Visibility = Visibility.Collapsed;
            WorldLoadModeUI.Visibility = Visibility.Collapsed;
            sugoMenu.Visibility = Visibility.Hidden;
            GameUI.Visibility = Visibility.Hidden;

            MainContent.Visibility = Visibility.Visible;
            MenuStack.Visibility = Visibility.Visible;
            MiniCard.chooseForDeck = false;

            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            MenuStack.BeginAnimation(OpacityProperty, fade);
        }

        private void BackToGameModeSelect(object sender, RoutedEventArgs e)
        {
            WorldCreateOrLoadUI.Visibility = Visibility.Collapsed;
            GameModeSelectUI.Visibility = Visibility.Visible;
        }

        private void BackToCreateOrLoad(object sender, RoutedEventArgs e)
        {
            CustomWorldSelectUI.Visibility = Visibility.Collapsed;
            WorldLoadUI.Visibility = Visibility.Collapsed;
            WorldCreateOrLoadUI.Visibility = Visibility.Visible;
        }

        private void BackToWorldLoadList(object sender, RoutedEventArgs e)
        {
            WorldLoadModeUI.Visibility = Visibility.Collapsed;
            WorldLoadUI.Visibility = Visibility.Visible;
        }

        private void StartCampaignSetup(object sender, RoutedEventArgs e)
        {
            string campaignFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, gameBaseFolder);
            string campaignFilePath = System.IO.Path.Combine(campaignFolder, defaultCampaignFile);

            try
            {
                Directory.CreateDirectory(campaignFolder);

                if (!File.Exists(campaignFilePath))
                {
                    string appDirIn = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultCampaignFile);
                    if (File.Exists(appDirIn))
                    {
                        File.Copy(appDirIn, campaignFilePath);
                    }
                    else
                    {
                        File.WriteAllText(campaignFilePath, "uj jatekos");
                    }
                }

                Vilag.EleresiUtvonal = campaignFolder;
                LoadWorld(defaultCampaignFile, false);

                GameModeSelectUI.Visibility = Visibility.Collapsed;
                DifficultySelectUI.Visibility = Visibility.Visible;
                DiffInput.Text = "0";
                DiffInput.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kritikus hiba a kampány betöltésekor: {ex.Message}", "Kritikus Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowWorldCreateOrLoad(object sender, RoutedEventArgs e)
        {
            GameModeSelectUI.Visibility = Visibility.Collapsed;
            WorldCreateOrLoadUI.Visibility = Visibility.Visible;
        }

        private void ShowWorldCreateOptions(object sender, RoutedEventArgs e)
        {
            WorldCreateOrLoadUI.Visibility = Visibility.Collapsed;
            CustomWorldSelectUI.Visibility = Visibility.Visible;
        }

        private void ShowWorldLoadList(object sender, RoutedEventArgs e)
        {
            WorldCreateOrLoadUI.Visibility = Visibility.Collapsed;
            WorldLoadUI.Visibility = Visibility.Visible;

            string savesPath = System.IO.Path.Combine(gameBaseFolder, savesFolder);
            Vilag.EleresiUtvonal = savesPath;

            try
            {
                WorldFilesList.Items.Clear();
                if (Directory.Exists(Vilag.EleresiUtvonal))
                {
                    string[] files = Directory.GetFiles(Vilag.EleresiUtvonal, "*.txt");
                    foreach (string file in files)
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        if (!fileName.EndsWith("_player.txt"))
                        {
                            WorldFilesList.Items.Add(fileName);
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(Vilag.EleresiUtvonal);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a világok listázásakor: {ex.Message}");
            }
        }

        private void WorldFile_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (WorldFilesList.SelectedItem == null) return;
            selectedWorldFile = WorldFilesList.SelectedItem.ToString();

            SelectedWorldText.Text = $"VILÁG: {selectedWorldFile}";
            WorldLoadUI.Visibility = Visibility.Collapsed;
            WorldLoadModeUI.Visibility = Visibility.Visible;
        }

        private void PlayWorld_Click(object sender, RoutedEventArgs e)
        {
            string savesPath = System.IO.Path.Combine(gameBaseFolder, savesFolder);
            Vilag.EleresiUtvonal = savesPath;
            LoadWorld(selectedWorldFile, false);
            WorldLoadModeUI.Visibility = Visibility.Collapsed;

            DifficultySelectUI.Visibility = Visibility.Visible;
            DiffInput.Text = "0";
            DiffInput.Focus();
        }

        private void EditWorld_Click(object sender, RoutedEventArgs e)
        {
            string savesPath = System.IO.Path.Combine(gameBaseFolder, savesFolder);
            Vilag.EleresiUtvonal = savesPath;
            LoadWorld(selectedWorldFile, true);
        }

        private void DeleteWorld_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedWorldFile)) return;

            MessageBoxResult result = MessageBox.Show(
                $"Biztosan törölni szeretnéd ezt a mentést?\n\n{selectedWorldFile}\n\nEz a művelet nem vonható vissza!",
                "Mentés Törlése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string savesPath = System.IO.Path.Combine(gameBaseFolder, savesFolder);
                    string filePath = System.IO.Path.Combine(savesPath, selectedWorldFile);
                    string playerFile = System.IO.Path.Combine(savesPath, System.IO.Path.GetFileNameWithoutExtension(selectedWorldFile) + "_player.txt");

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    if (File.Exists(playerFile))
                    {
                        File.Delete(playerFile);
                    }

                    MessageBox.Show("A mentés sikeresen törölve!", "Törlés Sikeres", MessageBoxButton.OK, MessageBoxImage.Information);

                    WorldLoadModeUI.Visibility = Visibility.Collapsed;
                    ShowWorldLoadList(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba a mentés törlésekor: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreateBaseWorld_Click(object sender, RoutedEventArgs e)
        {
            worldCreationMode = "base";
            CustomWorldSelectUI.Visibility = Visibility.Collapsed;
            WorldNameInputUI.Visibility = Visibility.Visible;
            WorldNameInput.Text = "";
            WorldNameInput.Focus();
        }

        private void CreateEmptyWorld_Click(object sender, RoutedEventArgs e)
        {
            worldCreationMode = "empty";
            CustomWorldSelectUI.Visibility = Visibility.Collapsed;
            WorldNameInputUI.Visibility = Visibility.Visible;
            WorldNameInput.Text = "";
            WorldNameInput.Focus();
        }

        private void BackToCustomWorldSelect(object sender, RoutedEventArgs e)
        {
            WorldNameInputUI.Visibility = Visibility.Collapsed;
            CustomWorldSelectUI.Visibility = Visibility.Visible;
        }

        private void ConfirmWorldName_Click(object sender, RoutedEventArgs e)
        {
            string newFileName = WorldNameInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(newFileName))
            {
                MessageBox.Show("A név nem lehet üres.");
                return;
            }

            if (!newFileName.EndsWith(".txt"))
            {
                newFileName += ".txt";
            }

            string savesPath = System.IO.Path.Combine(gameBaseFolder, savesFolder);
            Vilag.EleresiUtvonal = savesPath;

            string newPath = System.IO.Path.Combine(Vilag.EleresiUtvonal, newFileName);

            if (File.Exists(newPath))
            {
                MessageBox.Show("Egy világ már létezik ezen a néven. Válassz másikat.");
                return;
            }

            try
            {
                if (worldCreationMode == "base")
                {
                    string campaignFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, gameBaseFolder);
                    string campaignPath = System.IO.Path.Combine(campaignFolder, defaultCampaignFile);

                    if (File.Exists(campaignPath))
                    {
                        File.Copy(campaignPath, newPath);
                    }
                    else
                    {
                        File.WriteAllText(newPath, "uj jatekos");
                    }
                }
                else if (worldCreationMode == "empty")
                {
                    File.WriteAllText(newPath, "uj jatekos");
                }

                LoadWorld(newFileName, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba az új világ létrehozásakor: {ex.Message}");
            }
        }

        private void OpenWorldEditor()
        {
            isNavigating = true;
            NewWorld nw = new NewWorld(backgroundMusicPlayer);
            nw.Show();
            this.Close();
        }


        private void ConfirmDifficulty(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiffInput.Text, out int nehezseg) && nehezseg >= 0 && nehezseg <= 10)
            {
                Vilag.NehezsegiSzint = nehezseg;
                DifficultySelectUI.Visibility = Visibility.Collapsed;
                OpenDeckBuilder(null, null);
            }
            else
            {
                MessageBox.Show("A nehézségi szint 0 és 10 közötti egész szám kell legyen.", "Hibás Bemenet", MessageBoxButton.OK, MessageBoxImage.Warning);
                DiffInput.Focus();
            }
        }

        private void CancelDifficulty(object sender, RoutedEventArgs e)
        {
            DifficultySelectUI.Visibility = Visibility.Collapsed;
            GameModeSelectUI.Visibility = Visibility.Visible;
        }

        private void OpenDeckBuilder(object sender, RoutedEventArgs e)
        {
            MainContent.Visibility = Visibility.Hidden;
            GameUI.Visibility = Visibility.Visible;

            DeckBuilderUI.Visibility = Visibility.Visible;
            kazamataValasztas.Visibility = Visibility.Hidden;
            sugoMenu.Visibility = Visibility.Hidden;

            MiniCard.chooseForDeck = true;
            RefreshCardDisplays();
        }

        private void GoToKazamataSelect(object sender, RoutedEventArgs e)
        {
            if (Jatekos.pakli.Count == 0)
            {
                MessageBox.Show("A Deck üres! Válassz legalább egy lelket a harchoz.", "Üres Deck", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DeckBuilderUI.Visibility = Visibility.Hidden;
            kazamataValasztas.Visibility = Visibility.Visible;

            if (!KazamataKartyaUI.alreadyFilled) FillKazamata();
            KazamataKartyaUI.alreadyFilled = true;
        }

        private void BackToDeckBuilder(object sender, RoutedEventArgs e)
        {
            kazamataValasztas.Visibility = Visibility.Hidden;
            DeckBuilderUI.Visibility = Visibility.Visible;
        }

        private void BackToMenu(object sender, RoutedEventArgs e)
        {
            GameUI.Visibility = Visibility.Hidden;
            DeckBuilderUI.Visibility = Visibility.Hidden;
            kazamataValasztas.Visibility = Visibility.Hidden;
            sugoMenu.Visibility = Visibility.Hidden;
            GameModeSelectUI.Visibility = Visibility.Collapsed;
            DifficultySelectUI.Visibility = Visibility.Collapsed;
            CustomWorldSelectUI.Visibility = Visibility.Collapsed;
            WorldNameInputUI.Visibility = Visibility.Collapsed;
            WorldLoadUI.Visibility = Visibility.Collapsed;
            WorldCreateOrLoadUI.Visibility = Visibility.Collapsed;
            WorldLoadModeUI.Visibility = Visibility.Collapsed;

            MainContent.Visibility = Visibility.Visible;
            MenuStack.Visibility = Visibility.Visible;
            MiniCard.chooseForDeck = false;

            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            MenuStack.BeginAnimation(OpacityProperty, fade);
        }

        public void ShowSugo(object sender, RoutedEventArgs e)
        {
            MainContent.Visibility = Visibility.Hidden;
            GameUI.Visibility = Visibility.Visible;
            DeckBuilderUI.Visibility = Visibility.Hidden;
            kazamataValasztas.Visibility = Visibility.Hidden;
            sugoMenu.Visibility = Visibility.Visible;
        }

        private void FillKazamata()
        {
            kazamatak.Children.Clear();
            foreach (Kazamata k in Vilag.kazamatak)
            {
                kazamatak.Children.Add(new KazamataKartyaUI(k));
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void RefreshCardDisplays()
        {
            MiniCard.kartyak.Clear();
            GyujtemenyPanel.Children.Clear();
            PakliPanel.Children.Clear();

            foreach (Kartya k in Jatekos.gyujtemeny)
            {
                if (!Jatekos.pakli.Any(pakliKartya => pakliKartya.Nev == k.Nev))
                {
                    MiniCard card = new MiniCard(k);
                    card.SetInDeck(false);
                    GyujtemenyPanel.Children.Add(card);
                }
            }

            foreach (Kartya k in Jatekos.pakli)
            {
                MiniCard deckCard = new MiniCard(k);
                deckCard.SetInDeck(true);
                PakliPanel.Children.Add(deckCard);
            }

            int maxCapacity = (int)Math.Ceiling(((double)Jatekos.gyujtemeny.Count) / 2.0);
            DeckCountText.Text = $"{Jatekos.pakli.Count} / {maxCapacity} LÉLEK";

            if (Jatekos.pakli.Count > 0) ToBattleBtn.Opacity = 1;
            else ToBattleBtn.Opacity = 0.5;
        }

        private void Load()
        {
            string savesPath = System.IO.Path.Combine(gameBaseFolder, savesFolder);
            Directory.CreateDirectory(savesPath);
        }

        private void LoadWorld(string vilagFajlNev, bool openEditor)
        {
            Vilag.kartyak.Clear();
            Vilag.kazamatak.Clear();
            Jatekos.Reset();
            KazamataKartyaUI.alreadyFilled = false;

            Vilag.VilagFajlNev = vilagFajlNev;
            string filePath = System.IO.Path.Combine(Vilag.EleresiUtvonal, vilagFajlNev);

            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"A '{vilagFajlNev}' világfájl nem található.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                List<string[]> vezerek = new List<string[]>();
                List<string[]> kazamatakAdat = new List<string[]>();
                List<string[]> jatekosAdat = new List<string[]>();
                List<string> fileLines = File.ReadAllLines(filePath).ToList();

                foreach (string line in fileLines)
                {
                    try
                    {
                        string[] data = line.Split(';');
                        if (data.Length > 0 && !string.IsNullOrWhiteSpace(data[0]))
                        {
                            string command = data[0].Trim();

                            if (command == "kartya" && data.Length >= 5)
                            {
                                string[] convertedData = new string[] { "uj kartya", data[1], data[2], data[3], data[4] };
                                new Kartya(convertedData, true);
                            }
                            else if (command == "vezer" && data.Length >= 4)
                            {
                                string[] convertedData = new string[] { "uj vezer", data[1], data[2], data[3] };
                                vezerek.Add(convertedData);
                            }
                            else if (command == "kazamata")
                            {
                                string[] convertedData = new string[data.Length];
                                convertedData[0] = "uj kazamata";
                                for (int i = 1; i < data.Length; i++)
                                {
                                    convertedData[i] = data[i];
                                }
                                kazamatakAdat.Add(convertedData);
                            }
                            else if (command == "uj kartya" || command == "uj vezer" || command == "uj kazamata" ||
                                     command == "felvetel gyujtemenybe" || command == "uj pakli" || command == "uj jatekos")
                            {
                                switch (command)
                                {
                                    case "uj kartya":
                                        new Kartya(data, true);
                                        break;
                                    case "uj vezer":
                                        vezerek.Add(data);
                                        break;
                                    case "uj kazamata":
                                        kazamatakAdat.Add(data);
                                        break;
                                    case "felvetel gyujtemenybe":
                                    case "uj pakli":
                                    case "uj jatekos":
                                        jatekosAdat.Add(data);
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Hiba a '{line}' sor feldolgozásakor: {ex.Message}", "Betöltési Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }
                }

                foreach (string[] data in vezerek)
                {
                    try { new Kartya(data, true); }
                    catch (Exception ex) { MessageBox.Show($"Hiba vezér betöltésekor ('{string.Join(";", data)}'): {ex.Message}", "Betöltési Hiba", MessageBoxButton.OK, MessageBoxImage.Error); }
                }

                foreach (string[] data in kazamatakAdat)
                {
                    try { new Kazamata(data); }
                    catch (Exception ex) { MessageBox.Show($"Hiba kazamata betöltésekor ('{string.Join(";", data)}'): {ex.Message}", "Betöltési Hiba", MessageBoxButton.OK, MessageBoxImage.Error); }
                }

                foreach (string[] data in jatekosAdat)
                {
                    try
                    {
                        switch (data[0].Trim())
                        {
                            case "uj jatekos":
                                Jatekos.UjJatekos = true;
                                break;
                            case "felvetel gyujtemenybe":
                                Jatekos.FelvetelGyujtemeny(data);
                                break;
                            case "uj pakli":
                                Jatekos.UjPakli(data);
                                break;
                        }
                    }
                    catch (Exception ex) { MessageBox.Show($"Hiba játékosadat betöltésekor ('{string.Join(";", data)}'): {ex.Message}", "Betöltési Hiba", MessageBoxButton.OK, MessageBoxImage.Error); }
                }

                string playerFile = System.IO.Path.GetFileNameWithoutExtension(vilagFajlNev) + "_player.txt";
                string playerFilePath = System.IO.Path.Combine(Vilag.EleresiUtvonal, playerFile);

                if (File.Exists(playerFilePath))
                {
                    Jatekos.LoadJatekos(playerFile);
                }
                else
                {
                    if (Jatekos.gyujtemeny.Count == 0)
                    {
                        Jatekos.UjJatekos = true;
                        foreach (Kartya k in Vilag.kartyak)
                        {
                            if (k.SimaKartya)
                            {
                                Jatekos.gyujtemeny.Add(k.Clone());
                            }
                        }
                    }
                }

                if (openEditor)
                {
                    OpenWorldEditor();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kritikus hiba a '{vilagFajlNev}' fájl olvasásakor: {ex.Message}", "Kritikus Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            emberTimer?.Stop();

            if (!isNavigating)
            {
                backgroundMusicPlayer?.Stop();
                Application.Current.Shutdown();
            }
        }
    }
}