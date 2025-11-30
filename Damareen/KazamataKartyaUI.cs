using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Damareen
{
    internal class KazamataKartyaUI : Border
    {
        public Kazamata kazamata { get; set; }
        public static bool alreadyFilled { get; set; }
        public static Kazamata valasztott;

        public KazamataKartyaUI(Kazamata k)
        {
            this.kazamata = k;

            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#080808"));
            this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            this.BorderThickness = new Thickness(1);
            this.Height = 350;
            this.Width = 280;
            this.CornerRadius = new CornerRadius(4);
            this.Margin = new Thickness(25);
            this.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 30,
                ShadowDepth = 15,
                Opacity = 0.9
            };

            Grid p = new Grid();
            p.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            p.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel contentPanel = new StackPanel();
            contentPanel.VerticalAlignment = VerticalAlignment.Top;
            contentPanel.Margin = new Thickness(15);

            TextBlock l = new TextBlock()
            {
                Text = kazamata.KNev.ToUpper(),
                FontSize = 24,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A8A8A8")),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                FontWeight = FontWeights.Normal,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 30, 0, 20),
                FontFamily = new FontFamily("Times New Roman")
            };
            contentPanel.Children.Add(l);

            string nehezseg = "💀";
            switch (kazamata.KTipus)
            {
                case KTipusok.Kis:
                    nehezseg = "💀💀";
                    break;
                case KTipusok.Nagy:
                    nehezseg = "💀💀💀";
                    break;
            }

            Label e = new Label()
            {
                Content = nehezseg,
                FontSize = 48,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 20),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8a0303"))
            };
            e.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(138, 3, 3),
                BlurRadius = 40,
                ShadowDepth = 0,
                Opacity = 0.6
            };
            contentPanel.Children.Add(e);

            string jutalom = "♥ VITALITÁS";
            switch (kazamata.bonusz)
            {
                case Bonuszok.Sebzes:
                    jutalom = "⚔ ERŐ";
                    break;
                case Bonuszok.UjKartya:
                    jutalom = "📚 ELVESZETT LÉLEK";
                    break;
            }

            TextBlock j = new TextBlock()
            {
                Text = jutalom,
                FontSize = 18,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5A059")),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                FontWeight = FontWeights.Light,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 30, 0, 0)
            };
            contentPanel.Children.Add(j);

            Grid.SetRow(contentPanel, 0);
            p.Children.Add(contentPanel);

            Border buttonBorder = new Border()
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Height = 60,
                Margin = new Thickness(0)
            };

            Button b = new Button()
            {
                Content = "LÉPJ A KÖDBE",
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777")),
                FontSize = 20,
                FontWeight = FontWeights.Normal,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0)
            };
            b.Click += StartWar;

            buttonBorder.Child = b;
            Grid.SetRow(buttonBorder, 1);
            p.Children.Add(buttonBorder);

            this.Child = p;
            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += OnMouseLeave;
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8a0303"));
            this.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(138, 3, 3),
                BlurRadius = 60,
                ShadowDepth = 0,
                Opacity = 0.6
            };
            this.RenderTransform = new ScaleTransform(1.05, 1.05);
            this.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333"));
            this.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 30,
                ShadowDepth = 15,
                Opacity = 0.9
            };
            this.RenderTransform = new ScaleTransform(1, 1);
        }

        private void StartWar(object o, RoutedEventArgs e)
        {
            if (Jatekos.pakli.Count == 0)
            {
                MessageBoxResult m = MessageBox.Show("Nincsenek lelkeid a harchoz. Meglátogatod a hordozót?", "Üres", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (m == MessageBoxResult.Yes)
                {
                    (Application.Current.MainWindow as MainWindow)?.ShowSugo(null, null);
                }
            }
            else
            {
                if (this.kazamata.KTipus == KTipusok.Nagy)
                {
                    bool hasAllCards = true;
                    int simaKartyakSzama = Vilag.kartyak.Count(k => k.SimaKartya);
                    if (Jatekos.gyujtemeny.Count < simaKartyakSzama)
                    {
                        hasAllCards = false;
                    }

                    if (hasAllCards)
                    {
                        MessageBox.Show("Minden lelket megszereztél ezen a földön.", "Parázs Ura", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                valasztott = this.kazamata;
                Fight f = new Fight();
                f.ShowDialog();
            }
        }
    }
}