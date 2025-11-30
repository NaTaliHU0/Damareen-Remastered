using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Damareen
{
    public enum MiniCardMode { DeckBuilder, DungeonEditor_Available, DungeonEditor_InDungeon }

    public class MiniCard : Border
    {
        public Kartya kartya { get; set; }
        public string color { get; set; }
        public string accentColor { get; set; }
        public string icon { get; set; }
        public static bool chooseForDeck = false;
        public static bool alreadyLoaded = false;
        private bool inDeck = false;
        public static List<MiniCard> kartyak = new List<MiniCard>();
        public MiniCardMode Mode { get; set; }

        public bool IsInDeck() { return inDeck; }
        public void SetInDeck(bool value) { inDeck = value; }

        public MiniCard(Kartya k)
        {
            kartya = k;
            Mode = MiniCardMode.DeckBuilder;

            this.Focusable = false;
            this.FocusVisualStyle = null;

            switch (k.Kartyatipus)
            {
                case KartyaTipusok.Fire:
                    color = "#1a0a05";
                    accentColor = "#ff4500";
                    icon = "🔥";
                    break;
                case KartyaTipusok.Water:
                    color = "#050a1a";
                    accentColor = "#4169e1";
                    icon = "💧";
                    break;
                case KartyaTipusok.Earth:
                    color = "#0f0d05";
                    accentColor = "#8b7355";
                    icon = "⛰️";
                    break;
                case KartyaTipusok.Air:
                    color = "#0a0a0f";
                    accentColor = "#87ceeb";
                    icon = "🌪️";
                    break;
            }

            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            this.BorderThickness = new Thickness(2);
            this.Height = 240;
            this.Width = 170;
            this.CornerRadius = new CornerRadius(8);
            this.Margin = new Thickness(10);

            if (!kartya.SimaKartya)
            {
                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37"));
                this.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(212, 175, 55),
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.6
                };
            }
            else
            {
                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
                this.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 20,
                    ShadowDepth = 10,
                    Opacity = 0.8
                };
            }

            Grid mainGrid = new Grid { Width = 170, Height = 240 };
            mainGrid.Focusable = false;

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });

            Border topBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accentColor)),
                BorderThickness = new Thickness(0, 0, 0, 2),
                Padding = new Thickness(8, 5, 8, 5),
                Focusable = false
            };

            TextBlock nameBlock = new TextBlock
            {
                Text = k.Nev.ToUpper(),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("Times New Roman"),
                Focusable = false
            };

            topBorder.Child = nameBlock;
            Grid.SetRow(topBorder, 0);
            mainGrid.Children.Add(topBorder);

            StackPanel centerPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Focusable = false
            };

            if (!kartya.SimaKartya)
            {
                TextBlock crownIcon = new TextBlock
                {
                    Text = "👑",
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Opacity = 0.9,
                    Margin = new Thickness(0, 0, 0, 5),
                    Focusable = false
                };
                centerPanel.Children.Add(crownIcon);
            }

            Border iconBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                CornerRadius = new CornerRadius(50),
                Width = 80,
                Height = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accentColor)),
                BorderThickness = new Thickness(2),
                Focusable = false
            };

            TextBlock iconLabel = new TextBlock
            {
                Text = this.icon,
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0),
                Focusable = false
            };

            iconBorder.Child = iconLabel;
            centerPanel.Children.Add(iconBorder);

            Grid.SetRow(centerPanel, 1);
            mainGrid.Children.Add(centerPanel);

            Grid statsGrid = new Grid
            {
                Margin = new Thickness(10, 0, 10, 10),
                VerticalAlignment = VerticalAlignment.Bottom,
                Focusable = false
            };

            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Border attackBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(60, 255, 69, 0)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 0, 3, 0),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 69, 0)),
                BorderThickness = new Thickness(1),
                Focusable = false
            };

            StackPanel attackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Focusable = false
            };

            attackPanel.Children.Add(new TextBlock
            {
                Text = "⚔",
                FontSize = 16,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6347")),
                Margin = new Thickness(0, 0, 4, 0),
                Focusable = false
            });

            attackPanel.Children.Add(new TextBlock
            {
                Text = k.Attack.ToString(),
                FontSize = 18,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
                FontWeight = FontWeights.Bold,
                Focusable = false
            });

            attackBorder.Child = attackPanel;
            Grid.SetColumn(attackBorder, 0);
            statsGrid.Children.Add(attackBorder);

            Border hpBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(60, 220, 20, 60)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5),
                Margin = new Thickness(3, 0, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 220, 20, 60)),
                BorderThickness = new Thickness(1),
                Focusable = false
            };

            StackPanel hpPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Focusable = false
            };

            hpPanel.Children.Add(new TextBlock
            {
                Text = "♥",
                FontSize = 16,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dc143c")),
                Margin = new Thickness(0, 0, 4, 0),
                Focusable = false
            });

            hpPanel.Children.Add(new TextBlock
            {
                Text = k.Hp.ToString(),
                FontSize = 18,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
                FontWeight = FontWeights.Bold,
                Focusable = false
            });

            hpBorder.Child = hpPanel;
            Grid.SetColumn(hpBorder, 1);
            statsGrid.Children.Add(hpBorder);

            Grid.SetRow(statsGrid, 2);
            mainGrid.Children.Add(statsGrid);

            this.MouseDown += Click;
            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += OnMouseLeave;
            this.Child = mainGrid;

            kartyak.Add(this);
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (!kartya.SimaKartya)
            {
                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
                this.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(255, 215, 0),
                    BlurRadius = 35,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
            }
            else if (Mode == MiniCardMode.DeckBuilder && !inDeck)
            {
                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            }
            else if (Mode != MiniCardMode.DeckBuilder)
            {
                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            }

            this.RenderTransform = new ScaleTransform(1.05, 1.05);
            this.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (!kartya.SimaKartya)
            {
                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37"));
                this.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(212, 175, 55),
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.6
                };
            }
            else
            {
                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
                this.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 20,
                    ShadowDepth = 10,
                    Opacity = 0.8
                };
            }

            this.RenderTransform = new ScaleTransform(1, 1);
        }

        private void Click(object sender, EventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mw && (mw.DeckBuilderUI.IsVisible || chooseForDeck))
            {
                HandleDeckBuilderClick(mw);
            }
            else if (parentWindow is NewWorld nw && nw.DungeonCreationPanel.IsVisible)
            {
                nw.HandleDungeonCardClick(this.kartya, this, this.Mode);
            }
        }

        private void HandleDeckBuilderClick(MainWindow m)
        {
            if (m == null) return;

            Kartya clickedKartya = this.kartya;

            if (!clickedKartya.SimaKartya)
            {
                MessageBox.Show("Vezérkártya nem adható a paklihoz.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!inDeck)
            {
                if (Jatekos.pakli.Count < Math.Ceiling(((double)Jatekos.gyujtemeny.Count) / 2.0))
                {
                    Jatekos.pakli.Add(clickedKartya);
                    m.RefreshCardDisplays();
                }
                else
                {
                    MessageBox.Show("A Deck megtelt. Nem köthetsz magadhoz több lelket.", "Megtelt", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                Kartya kartyaToRemove = Jatekos.pakli.FirstOrDefault(k => k.Nev == clickedKartya.Nev);
                if (kartyaToRemove != null)
                {
                    Jatekos.pakli.Remove(kartyaToRemove);
                }
                m.RefreshCardDisplays();
            }
        }

        public static void LoadDeck()
        {
            MainWindow m = Application.Current.MainWindow as MainWindow;
            if (m != null) m.RefreshCardDisplays();
        }
    }

}
