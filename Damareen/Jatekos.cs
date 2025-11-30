using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Damareen
{
    internal static class Jatekos
    {
        public static List<Kartya> gyujtemeny = new List<Kartya>();
        public static List<Kartya> pakli = new List<Kartya>();
        private static List<string> hozzaadott = new List<string>();
        public static bool UjJatekos = false;

        public class Save
        {
            public List<Kartya> Gyujtemeny { get; set; }
            public List<Kartya> Pakli { get; set; }
            public List<string> Hozzaadott = new List<string>();
            public Save()
            {
                this.Gyujtemeny = new List<Kartya>(gyujtemeny);
                this.Pakli = pakli;
                this.Hozzaadott = hozzaadott;
            }
        }

        public static void Reset()
        {
            gyujtemeny.Clear();
            pakli.Clear();
            hozzaadott.Clear();
            UjJatekos = false;
        }

        public static void FelvetelGyujtemeny(string[] adatok)
        {
            if (UjJatekos)
            {
                if (adatok.Length < 2) throw new Exception("Hiányzó adat a 'felvetel gyujtemenybe' parancsnál.");

                string kartyaNev = adatok[1];
                Kartya k = Vilag.kartyak.FirstOrDefault(c => c.Nev == kartyaNev);

                if (k != null)
                {
                    if (!hozzaadott.Contains(kartyaNev))
                    {
                        if (k.SimaKartya)
                        {
                            gyujtemeny.Add(k.Clone());
                            hozzaadott.Add(kartyaNev);
                        }
                        else
                        {
                            Console.WriteLine($"Figyelmeztetés: '{k.Nev}' vezérkártya nem adható a gyűjteményhez.");
                        }
                    }
                }
                else
                {
                    throw new Exception($"Nem található kártya: '{kartyaNev}' a 'felvetel gyujtemenybe' parancshoz.");
                }
            }
            else throw new Exception("Nem hozott létre új játékost!");
        }

        public static void UjPakli(string[] adatok)
        {
            if (UjJatekos)
            {
                if (adatok.Length < 2) throw new Exception("Hiányzó adat az 'uj pakli' parancsnál.");

                string[] karakterek = adatok[1].Split(',');
                pakli = new List<Kartya>();
                if (gyujtemeny.Count == 0) throw new Exception("A gyűjtemény még üres!");
                if (karakterek.Length > Math.Ceiling(((double)gyujtemeny.Count) / 2.0))
                {
                    throw new Exception("A pakli hosszabb mint a gyűjtemény fele!");
                }
                foreach (string x in karakterek)
                {
                    Kartya k = gyujtemeny.FirstOrDefault(c => c.Nev == x);
                    if (k != null)
                    {
                        if (k.SimaKartya)
                        {
                            pakli.Add(k);
                        }
                    }
                }
            }
            else throw new Exception("Nem hozott létre új játékost!");
        }

        public static void ExportJatekos(string file)
        {
            if (string.IsNullOrEmpty(Vilag.EleresiUtvonal)) throw new Exception("Nincs beállítva elérési útvonal a játékos mentéséhez.");

            try
            {
                StreamWriter sw = new StreamWriter(Path.Combine(Vilag.EleresiUtvonal, file));
                foreach (Kartya k in gyujtemeny)
                {
                    if (k.SimaKartya)
                    {
                        k.UpdateArrayData();
                        sw.WriteLine($"gyujtemeny;{k.Nev};{k.Attack};{k.Hp};{k.Tipus()}");
                    }
                }
                sw.WriteLine();
                foreach (Kartya k in pakli)
                {
                    sw.WriteLine($"pakli;{k.Nev}");
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba a játékos exportálásakor: {ex.Message}");
            }
        }

        public static void LoadJatekos(string file)
        {
            if (string.IsNullOrEmpty(Vilag.EleresiUtvonal)) throw new Exception("Nincs beállítva elérési útvonal a játékos betöltéséhez.");

            string filePath = Path.Combine(Vilag.EleresiUtvonal, file);
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                gyujtemeny.Clear();
                pakli.Clear();
                hozzaadott.Clear();

                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] data = line.Split(';');
                    if (data.Length == 0) continue;

                    if (data[0] == "gyujtemeny" && data.Length >= 5)
                    {
                        string nev = data[1];
                        int attack = int.Parse(data[2]);
                        int hp = int.Parse(data[3]);
                        string tipus = data[4];

                        Kartya baseCard = Vilag.kartyak.FirstOrDefault(k => k.Nev == nev);
                        if (baseCard != null && baseCard.SimaKartya)
                        {
                            Kartya loadedCard = baseCard.Clone();
                            loadedCard.Attack = attack;
                            loadedCard.Hp = hp;
                            loadedCard.CurrentHp = hp;
                            loadedCard.UpdateArrayData();

                            gyujtemeny.Add(loadedCard);
                            hozzaadott.Add(nev);
                        }
                    }
                    else if (data[0] == "pakli" && data.Length >= 2)
                    {
                        string nev = data[1];
                        Kartya k = gyujtemeny.FirstOrDefault(c => c.Nev == nev);
                        if (k != null)
                        {
                            pakli.Add(k);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba a játékos betöltésekor: {ex.Message}");
            }
        }
    }
}