using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Damareen
{
    public partial class Fight : Window
    {
        public Fight()
        {
            InitializeComponent();
            FillCards();
        }

        private void FillCards()
        {
            if (KazamataKartyaUI.valasztott == null)
            {
                MessageBox.Show("Hiba: Nem sikerült betölteni a kazamatát.");
                this.Close();
                return;
            }

            KazamataNev.Text = $"{KazamataKartyaUI.valasztott.KNev.ToUpper()}";

            foreach (Kartya k in Jatekos.pakli)
            {
                this.JatekosKartyak.Children.Add(new MiniCard(k));
            }

            for (int i = 0; i < KazamataKartyaUI.valasztott.KKartyak.Count; i++)
            {
                this.KazamataKartyak.Children.Add(new MiniCard(KazamataKartyaUI.valasztott.KKartyak[i]));
            }

            string harcFajl = "harc01.txt";
            string harcPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", harcFajl);

            string[] x = $"harc;{KazamataKartyaUI.valasztott.KNev};{harcPath}".Split(';');
            Kazamata.UjHarc(x);

            if (!string.IsNullOrEmpty(Vilag.EleresiUtvonal) && !string.IsNullOrEmpty(Vilag.VilagFajlNev))
            {
                try
                {
                    Vilag.ExportVilag(Vilag.VilagFajlNev);

                    string jatekosFajl = System.IO.Path.GetFileNameWithoutExtension(Vilag.VilagFajlNev) + "_player.txt";
                    Jatekos.ExportJatekos(jatekosFajl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Automatikus mentés sikertelen: {ex.Message}");
                }
            }

            Status.Text = FormatNaplo(Kazamata.Naplo);
        }

        private string FormatNaplo(string naplo)
        {
            StringBuilder formatted = new StringBuilder();
            if (string.IsNullOrEmpty(naplo)) return "A csata naplója üres.";

            string[] sorok = naplo.Split('\n');

            foreach (string sor in sorok)
            {
                if (string.IsNullOrWhiteSpace(sor)) continue;

                string[] reszek = sor.Split(';');
                if (reszek.Length == 0) continue;

                if (reszek[0] == "harc kezdodik" && reszek.Length > 1)
                {
                    formatted.AppendLine($"\nA SÖTÉTSÉG MEGMOZDUL...");
                    formatted.AppendLine($"Helyszín: {reszek[1]}");
                    formatted.AppendLine($"------------------------------------\n");
                }
                else if (reszek[0].Contains(".kor") && reszek.Length > 2)
                {
                    string ki = reszek[1];
                    string akcio = reszek[2];

                    if (akcio == "kijatszik" && reszek.Length >= 7)
                    {
                        string nev = reszek[3];
                        string sebzes = reszek[4];
                        string hp = reszek[5];
                        string tipus = reszek[6];

                        string tipusIcon = GetTipusIcon(tipus);
                        string kiIcon = ki == "kazamata" ? "💀" : "🛡️";
                        string subject = ki == "kazamata" ? "A MÉLYSÉG" : "HARCOS";

                        formatted.AppendLine($"{kiIcon} {subject} megidézi: {nev} {tipusIcon}");
                        formatted.AppendLine($"   TÁM: {sebzes} | ÉLET: {hp}");
                    }
                    else if (akcio == "tamad" && reszek.Length >= 7)
                    {
                        string tamado = reszek[3];
                        string sebzes = reszek[4];
                        string vedo = reszek[5];
                        string maradtHp = reszek[6];

                        bool kritikus = false;
                        bool kitert = false;

                        for (int i = 7; i < reszek.Length; i++)
                        {
                            if (reszek[i] == "kritikus") kritikus = true;
                            if (reszek[i] == "kitert") kitert = true;
                        }

                        if (kitert)
                        {
                            formatted.AppendLine($"   🌪️ KITÉRÉS! {vedo} sikeresen kikerülte {tamado} támadását!");
                        }
                        else
                        {
                            string attackPrefix = kritikus ? "⚔ KRITIKUS CSAPÁS!" : "   ⚔ Csapás!";
                            formatted.AppendLine($"{attackPrefix} {tamado} {sebzes} sebzést okoz neki: {vedo}");
                            formatted.AppendLine($"   {vedo} életereje: {maradtHp}");

                            if (maradtHp == "0")
                            {
                                formatted.AppendLine($"   💀 {vedo} ELESETT.");
                            }
                        }
                        formatted.AppendLine();
                    }
                }
                else if (reszek[0] == "jatekos nyert")
                {
                    formatted.AppendLine($"\n====================================");
                    formatted.AppendLine($"       G Y Ő Z E L E M   E L É R V E");
                    formatted.AppendLine($"====================================\n");

                    MainWindow mw = Application.Current.MainWindow as MainWindow;

                    if (reszek.Length == 2)
                    {
                        formatted.AppendLine($"Elragadott lélek: {reszek[1]}");
                        mw?.Dispatcher.Invoke(() => {
                            mw.RefreshCardDisplays();
                        });
                    }
                    else if (reszek.Length == 3)
                    {
                        string bonusz = "ERŐ";
                        if (reszek[1] == "eletero") bonusz = "VITALITÁS";

                        formatted.AppendLine($"Máglya fellobbant: {bonusz} NÖVEKEDÉS");
                        formatted.AppendLine($"Átitatva: {reszek[2]}");
                        mw?.Dispatcher.Invoke(() => {
                            mw.RefreshCardDisplays();
                        });
                    }
                }
                else if (reszek[0] == "jatekos vesztett")
                {
                    formatted.AppendLine($"\n====================================");
                    formatted.AppendLine($"            M E G H A L T Á L");
                    formatted.AppendLine($"====================================");
                }
                else if (reszek[0] == "GYÓGYULÁS" && reszek.Length >= 5)
                {
                    formatted.Remove(formatted.Length-1, 1);
                    formatted.AppendLine($"   💧 VÍZI REGENERÁCIÓ: {reszek[1]} gyógyul +{reszek[2]} HP (Élet: {reszek[4]})");
                }
            }

            return formatted.ToString();
        }

        private string GetTipusIcon(string tipus)
        {
            switch (tipus.ToLower())
            {
                case "tuz": return "🔥";
                case "viz": return "💧";
                case "fold": return "⛰️";
                case "levego": return "🌪️";
                default: return "";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}