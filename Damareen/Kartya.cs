using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Damareen
{
    public enum KartyaTipusok { Fire, Water, Earth, Air }
    public class Kartya
    {
        public KartyaTipusok Kartyatipus { get; set; }
        public bool SimaKartya { get; set; }
        public int Hp { get; set; }
        public int CurrentHp { get; set; }
        public int Attack { get; set; }
        public string Nev { get; set; }
        public string[] a;
        private static Random random = new Random(Guid.NewGuid().GetHashCode());

        public bool WaterHealedThisRound { get; set; } = false;

        public Kartya(string[] adatok, bool notClone)
        {
            if (adatok.Length < 1) throw new Exception("Üres kártya adat.");
            this.a = adatok;
            string command = adatok[0].Trim();

            if (command == "uj vezer" && adatok.Length == 4)
            {
                SimaKartya = false;
                if (adatok[1].Trim().Length > 16)
                {
                    throw new Exception($"A vezérkártya neve ('{adatok[1].Trim()}') hosszabb a lehetségesnél (max 16 karakter).");
                }
                Nev = adatok[1].Trim();

                string alapKartyaNev = adatok[2].Trim();
                string bonuszTipus = adatok[3].Trim();

                Kartya alapKartya = Vilag.kartyak.FirstOrDefault(k => k.Nev == alapKartyaNev) ?? throw new Exception($"Vezér alap kártya ('{alapKartyaNev}') nem található a '{Nev}' létrehozásához.");
                if (!alapKartya.SimaKartya)
                {
                    throw new Exception($"Vezérkártya ('{Nev}') csak sima kártyából ('{alapKartyaNev}') származtatható.");
                }

                Hp = alapKartya.Hp;
                CurrentHp = alapKartya.Hp;
                Attack = alapKartya.Attack;
                Kartyatipus = alapKartya.Kartyatipus;
                WaterHealedThisRound = false;

                if (bonuszTipus == "eletero")
                {
                    Hp *= 2;
                    CurrentHp *= 2;
                }
                else if (bonuszTipus == "sebzes")
                {
                    Attack *= 2;
                }
                else
                {
                    throw new Exception($"Ismeretlen vezérkártya bónusz típus: '{bonuszTipus}' a '{Nev}' kártyánál.");
                }
            }
            else if (command == "uj kartya" && adatok.Length == 5)
            {
                SimaKartya = true;

                if (adatok[1].Trim().Length <= 16) Nev = adatok[1].Trim();
                else throw new Exception($"A kártya neve ('{adatok[1].Trim()}') hosszabb a lehetségesnél (max 16 karakter).");

                if (int.TryParse(adatok[2], out int s))
                {
                    if (s < 2 || s > 100)
                    {
                        throw new Exception($"A sebzés ('{s}') értéke 2 és 100 között kell legyen!");
                    }
                    Attack = s;
                }
                else throw new Exception($"A sebzés ('{adatok[2]}') nem megfelelő formátumú!");

                if (int.TryParse(adatok[3], out int hp))
                {
                    if (hp < 1 || hp > 100)
                    {
                        throw new Exception($"Az életerő ('{hp}') értéke 1 és 100 között kell legyen!");
                    }
                    Hp = hp;
                    CurrentHp = Hp;
                }
                else throw new Exception($"A hp ('{adatok[3]}') nem megfelelő formátumú!");

                switch (adatok[4].Trim())
                {
                    case "tuz":
                        Kartyatipus = KartyaTipusok.Fire;
                        break;
                    case "viz":
                        Kartyatipus = KartyaTipusok.Water;
                        break;
                    case "fold":
                        Kartyatipus = KartyaTipusok.Earth;
                        break;
                    case "levego":
                        Kartyatipus = KartyaTipusok.Air;
                        break;
                    default:
                        throw new Exception($"Ismeretlen kártya típus: {adatok[4]}");
                }
            }
            else
            {
                throw new Exception($"Ismeretlen vagy hibás kártya formátum: {string.Join(";", adatok)}");
            }


            if (notClone) Vilag.kartyak.Add(this);
        }

        public string Tipus()
        {
            switch (this.Kartyatipus)
            {
                case KartyaTipusok.Fire:
                    return "tuz";
                case KartyaTipusok.Water:
                    return "viz";
                case KartyaTipusok.Earth:
                    return "fold";
                case KartyaTipusok.Air:
                    return "levego";
            }
            return "";
        }

        public Kartya Clone()
        {
            Kartya clone = new Kartya(this.a, false)
            {
                Hp = this.Hp,
                Attack = this.Attack,
                CurrentHp = this.CurrentHp,
                WaterHealedThisRound = false
            };
            return clone;
        }

        public void UpdateArrayData()
        {
            if (SimaKartya && a != null && a.Length >= 5)
            {
                a[2] = Attack.ToString();
                a[3] = Hp.ToString();
            }
        }

        public int CalculateAttackDamage(out bool wasCrit)
        {
            wasCrit = false;
            if (Kartyatipus == KartyaTipusok.Fire && App.UIFlag)
            {
                if (random.Next(100) < 25)
                {
                    wasCrit = true;
                    return (int)(Attack * 1.5);
                }
            }
            return Attack;
        }

        public int ApplyWaterHeal()
        {
            if (Kartyatipus == KartyaTipusok.Water && !WaterHealedThisRound)
            {
                if (App.UIFlag)
                {
                    int oldHp = CurrentHp;
                    CurrentHp = Math.Min(CurrentHp + (int)(Hp * 0.25), Hp);
                    WaterHealedThisRound = true;
                    return CurrentHp - oldHp;
                }
                else
                {
                    // Still mark as healed even in non-UI mode to prevent repeated attempts
                    WaterHealedThisRound = true;
                }
            }
            return 0;
        }

        public int CalculateIncomingDamage(int incomingDamage)
        {
            if (Kartyatipus == KartyaTipusok.Earth && App.UIFlag)
            {
                return (int)Math.Ceiling(incomingDamage * 0.75);
            }
            return incomingDamage;
        }

        public bool TryDodge()
        {
            if (Kartyatipus == KartyaTipusok.Air && App.UIFlag)
            {
                return random.Next(100) < 25;
            }
            return false;
        }
    }
}