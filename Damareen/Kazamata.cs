using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Damareen
{
    public enum KTipusok { Egyszeru, Kis, Nagy }
    public enum Bonuszok { Sebzes, Eletero, UjKartya }

    internal class Kazamata
    {
        public string KNev { get; set; }
        public KTipusok KTipus { get; set; }
        public Bonuszok bonusz { get; set; }
        public int BonuszErtek { get; set; }
        public string BonuszKartyaNev { get; set; }
        public List<Kartya> KKartyak = new List<Kartya>();
        public static string Naplo;
        public string[] a;
        private static Random diffRand = new Random();

        public Kazamata(string nev, KTipusok tipus)
        {
            if (nev.Length > 20)
            {
                throw new Exception($"A kazamata neve ('{nev}') hosszabb a lehetségesnél (max 20 karakter).");
            }
            this.KNev = nev;
            this.KTipus = tipus;
            this.KKartyak = new List<Kartya>();
            this.BonuszErtek = 0;
            this.BonuszKartyaNev = "";
            Vilag.kazamatak.Add(this);
        }

        public Kazamata(string[] tomb)
        {
            if (tomb.Length < 5) throw new Exception($"Hiányos 'uj kazamata' definíció: {string.Join(";", tomb)}");

            this.a = tomb;
            KNev = tomb[2];
            if (KNev.Length > 20)
            {
                throw new Exception($"A kazamata neve ('{KNev}') hosszabb a lehetségesnél (max 20 karakter).");
            }

            switch (tomb[1])
            {
                case "egyszeru": KTipus = KTipusok.Egyszeru; break;
                case "kis": KTipus = KTipusok.Kis; break;
                case "nagy": KTipus = KTipusok.Nagy; break;
                default: throw new Exception($"Ismeretlen kazamata típus: {tomb[1]} ({KNev})");
            }

            string[] simaKartyaNevek = tomb[3].Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            int vezerCount = 0;

            if (KTipus == KTipusok.Egyszeru)
            {
                if (simaKartyaNevek.Length != 1) throw new Exception($"Az 'egyszeru' kazamatának ({KNev}) pontosan 1 sima kártyát kell tartalmaznia.");
                if (tomb.Length < 5) throw new Exception($"Hiányos 'egyszeru' kazamata definíció: {KNev}");
                ParseBonusz(tomb[4]);
            }
            else if (KTipus == KTipusok.Kis)
            {
                if (simaKartyaNevek.Length != 3) throw new Exception($"A 'kis' kazamatának ({KNev}) pontosan 3 sima kártyát kell tartalmaznia.");
                if (tomb.Length < 6) throw new Exception($"Hiányos 'kis' kazamata definíció: {KNev}");
                AddVezer(tomb[4]);
                vezerCount = 1;
                ParseBonusz(tomb[5]);
            }
            else if (KTipus == KTipusok.Nagy)
            {
                if (simaKartyaNevek.Length != 5) throw new Exception($"A 'nagy' kazamatának ({KNev}) pontosan 5 sima kártyát kell tartalmaznia.");
                if (tomb.Length < 5) throw new Exception($"Hiányos 'nagy' kazamata definíció: {KNev}");
                AddVezer(tomb[4]);
                vezerCount = 1;
                this.bonusz = Bonuszok.UjKartya;
                this.BonuszKartyaNev = "";
            }

            foreach (string item in simaKartyaNevek)
            {
                if ((Vilag.kartyak.FirstOrDefault(card => card.Nev == item.Trim())) is Kartya k)
                {
                    if (!k.SimaKartya) throw new Exception($"Kazamata sima kártya listája ('{item}') vezérkártyát tartalmaz. ({KNev})");
                    KKartyak.Add(k.Clone());
                }
                else throw new Exception($"Kazamata kártya (sima) nem található: '{item}' ({KNev})");
            }

            int totalExpected = simaKartyaNevek.Length + vezerCount;
            if (KKartyak.Count != totalExpected)
            {
                throw new Exception($"Kazamata kártyaszám hiba: {KKartyak.Count} kártya lett betöltve a várt {totalExpected} helyett. ({KNev})");
            }

            Vilag.kazamatak.Add(this);
        }

        private void AddVezer(string vezerNev)
        {
            if (string.IsNullOrWhiteSpace(vezerNev))
            {
                throw new Exception($"Hiányzó vezér definíció. ({KNev})");
            }
            if ((Vilag.kartyak.FirstOrDefault(card => card.Nev == vezerNev.Trim())) is Kartya k)
            {
                if (k.SimaKartya) throw new Exception($"A(z) '{vezerNev}' sima kártya, nem lehet vezér. ({KNev})");
                KKartyak.Add(k.Clone());
            }
            else throw new Exception($"Vezér kártya nem található: '{vezerNev}' ({KNev})");
        }

        private void ParseBonusz(string bonuszString)
        {
            switch (bonuszString)
            {
                case "eletero":
                    this.bonusz = Bonuszok.Eletero;
                    this.BonuszErtek = 2;
                    break;
                case "sebzes":
                    this.bonusz = Bonuszok.Sebzes;
                    this.BonuszErtek = 1;
                    break;
                default:
                    throw new Exception($"Ismeretlen vagy érvénytelen bónusz 'egyszeru'/'kis' kazamatánál: {bonuszString} ({KNev}). Csak 'eletero' vagy 'sebzes' lehet.");
            }
        }

        public static void UjHarc(string[] data)
        {
            if (data.Length != 3)
            {
                Console.WriteLine($"Hibás 'harc' parancs formátum. Várt: harc;kazamata_neve;fajl_neve");
                return;
            }

            string kazamataNev = data[1].Trim();
            string fajlNev = data[2].Trim();

            foreach (Kazamata k in Vilag.kazamatak)
            {
                if (k.KNev == kazamataNev)
                {
                    Harc(Jatekos.pakli, k, fajlNev);
                    return;
                }
            }

            Console.WriteLine($"Kazamata nem található: '{kazamataNev}'");
        }

        private static void Harc(List<Kartya> pakli, Kazamata valasztottKazamata, string file)
        {
            double n = (double)Vilag.NehezsegiSzint;
            List<Kartya> tpakli;
            List<Kartya> tkazamata;

            if (Vilag.NehezsegiSzint > 0)
            {
                tpakli = CloneAndAdjust(pakli, forPlayer: true, n);
                tkazamata = CloneAndAdjust(valasztottKazamata.KKartyak, forPlayer: false, n);
            }
            else
            {
                tpakli = new List<Kartya>();
                foreach (Kartya k in pakli)
                {
                    Kartya klon = k.Clone();
                    klon.CurrentHp = klon.Hp;
                    tpakli.Add(klon);
                }

                tkazamata = new List<Kartya>();
                foreach (Kartya k in valasztottKazamata.KKartyak)
                {
                    Kartya klon = k.Clone();
                    klon.CurrentHp = klon.Hp;
                    tkazamata.Add(klon);
                }
            }

            Naplo = $"harc kezdodik;{valasztottKazamata.KNev}\n\n";

            bool kazamataKiJatszik = true;
            bool jatekosKiJatszik = true;
            int kor = 1;

            do
            {
                bool jatekosLepett = false;
                bool kazamataLepett = false;
                bool kazamataDamaged = false;
                bool jatekosDamaged = false;
                if (kazamataKiJatszik && !kazamataLepett)
                {
                    kazamataKiJatszik = false;
                    Kartya playedCard = tkazamata[0];

                    AppendKijatszikLog(kor, "kazamata", playedCard, 0, doubleLine: false);
                    kazamataLepett = true;
                }
                if (!kazamataKiJatszik && !kazamataLepett)
                {
                    Kartya tamado = tkazamata[0];
                    Kartya vedo = tpakli[0];

                    int vegsoSebzes;
                    bool wasCrit = false;
                    bool kitert = false;

                    if (Vilag.NehezsegiSzint > 0)
                    {
                        vegsoSebzes = ResolveAttack(tamado, vedo, out wasCrit, out kitert);
                    }
                    else
                    {
                        kitert = vedo.TryDodge();
                        if (!kitert)
                        {
                            double arany = TamadasArany(tamado, vedo);
                            int alapSebzes = tamado.CalculateAttackDamage(out wasCrit);
                            int sebzesArannyel = (int)Math.Ceiling(alapSebzes * arany);
                            vegsoSebzes = vedo.CalculateIncomingDamage(sebzesArannyel);
                        }
                        else
                        {
                            vegsoSebzes = 0;
                        }
                    }

                    if (!kitert)
                    {
                        vedo.CurrentHp -= vegsoSebzes;
                        if (vedo.CurrentHp < 0) vedo.CurrentHp = 0;
                        if (vegsoSebzes > 0) jatekosDamaged = true;
                    }

                    Naplo += $"{kor}.kor;kazamata;tamad;{tamado.Nev};{vegsoSebzes};{vedo.Nev};{vedo.CurrentHp}";
                    if (wasCrit) Naplo += ";kritikus";
                    if (kitert) Naplo += ";kitert";
                    Naplo += "\n";
                    if (App.UIFlag && jatekosDamaged && vedo.CurrentHp > 0)
                    {
                        vedo.WaterHealedThisRound = false;
                        int healAmount = vedo.ApplyWaterHeal();
                        if (healAmount > 0)
                        {
                            Naplo += $"GYÓGYULÁS;{vedo.Nev};{healAmount};t gyógyult HP;{vedo.CurrentHp}\n";
                        }
                    }

                    kazamataLepett = true;
                }
                if (tpakli[0].CurrentHp <= 0)
                {
                    jatekosKiJatszik = true;
                    tpakli.RemoveAt(0);
                    if (tpakli.Count == 0)
                    {
                        Naplo += "\njatekos vesztett";
                        break;
                    }
                }
                if (jatekosKiJatszik && !jatekosLepett)
                {
                    jatekosKiJatszik = false;
                    Kartya playedCard = tpakli[0];

                    AppendKijatszikLog(kor, "jatekos", playedCard, 0, doubleLine: true);
                    jatekosLepett = true;
                }
                if (!jatekosKiJatszik && !jatekosLepett)
                {
                    Kartya tamado = tpakli[0];
                    Kartya vedo = tkazamata[0];

                    int vegsoSebzes;
                    bool wasCrit = false;
                    bool kitert = false;

                    if (Vilag.NehezsegiSzint > 0)
                    {
                        vegsoSebzes = ResolveAttack(tamado, vedo, out wasCrit, out kitert);
                    }
                    else
                    {
                        kitert = vedo.TryDodge();
                        if (!kitert)
                        {
                            double arany = TamadasArany(tamado, vedo);
                            int alapSebzes = tamado.CalculateAttackDamage(out wasCrit);
                            int sebzesArannyel = (int)Math.Ceiling(alapSebzes * arany);
                            vegsoSebzes = vedo.CalculateIncomingDamage(sebzesArannyel);
                        }
                        else
                        {
                            vegsoSebzes = 0;
                        }
                    }

                    if (!kitert)
                    {
                        vedo.CurrentHp -= vegsoSebzes;
                        if (vedo.CurrentHp < 0) vedo.CurrentHp = 0;
                        if (vegsoSebzes > 0) kazamataDamaged = true;
                    }

                    Naplo += $"{kor}.kor;jatekos;tamad;{tamado.Nev};{vegsoSebzes};{vedo.Nev};{vedo.CurrentHp}";
                    if (wasCrit) Naplo += ";kritikus";
                    if (kitert) Naplo += ";kitert";
                    Naplo += "\n";
                    if (App.UIFlag && kazamataDamaged && vedo.CurrentHp > 0)
                    {
                        vedo.WaterHealedThisRound = false;
                        int healAmount = vedo.ApplyWaterHeal();
                        if (healAmount > 0)
                        {
                            Naplo += $"GYÓGYULÁS;{vedo.Nev};{healAmount};t gyógyult HP;{vedo.CurrentHp}\n";
                        }
                    }

                    jatekosLepett = true;
                }
                if (tkazamata[0].CurrentHp <= 0)
                {
                    kazamataKiJatszik = true;
                    tkazamata.RemoveAt(0);
                    if (tkazamata.Count == 0)
                    {
                        string elsoTuleloPakliKartyaNev = tpakli.Count > 0 ? tpakli[0].Nev : null;
                        Kartya eredetiGyujtemenyKartya = null;

                        if (!string.IsNullOrEmpty(elsoTuleloPakliKartyaNev))
                        {
                            eredetiGyujtemenyKartya = Jatekos.gyujtemeny.FirstOrDefault(gk => gk.Nev == elsoTuleloPakliKartyaNev);
                        }

                        if (valasztottKazamata.KTipus == KTipusok.Nagy)
                        {
                            Kartya ujKartya = null;
                            foreach (Kartya k in Vilag.kartyak)
                            {
                                if (k.SimaKartya && !Jatekos.gyujtemeny.Any(x => x.Nev == k.Nev))
                                {
                                    ujKartya = k.Clone();
                                    break;
                                }
                            }

                            if (ujKartya != null && !Jatekos.gyujtemeny.Any(k => k.Nev == ujKartya.Nev))
                            {
                                Jatekos.gyujtemeny.Add(ujKartya);
                                Naplo += $"\njatekos nyert;{ujKartya.Nev}";
                            }
                            else
                            {
                                Naplo += "\njatekos nyert";
                            }
                        }
                        else
                        {
                            if (eredetiGyujtemenyKartya == null)
                            {
                                Naplo += "\njatekos nyert";
                                break;
                            }

                            if (valasztottKazamata.bonusz == Bonuszok.Sebzes)
                            {
                                eredetiGyujtemenyKartya.Attack += valasztottKazamata.BonuszErtek;
                                eredetiGyujtemenyKartya.UpdateArrayData();
                                Naplo += $"\njatekos nyert;sebzes;{eredetiGyujtemenyKartya.Nev}";
                            }
                            else if (valasztottKazamata.bonusz == Bonuszok.Eletero)
                            {
                                eredetiGyujtemenyKartya.Hp += valasztottKazamata.BonuszErtek;
                                eredetiGyujtemenyKartya.CurrentHp = eredetiGyujtemenyKartya.Hp;
                                eredetiGyujtemenyKartya.UpdateArrayData();
                                Naplo += $"\njatekos nyert;eletero;{eredetiGyujtemenyKartya.Nev}";
                            }
                        }
                        break;
                    }
                }

                if (jatekosLepett && kazamataLepett)
                {
                    Naplo += "\n";
                }

                kor++;
            } while (tpakli.Count > 0 && tkazamata.Count > 0);

            PrintHarc(Naplo, file);
        }
        private static List<Kartya> CloneAndAdjust(IEnumerable<Kartya> source, bool forPlayer, double n)
        {
            var list = new List<Kartya>();
            foreach (Kartya k in source)
            {
                Kartya klon = k.Clone();
                double mod = forPlayer ? 1.0 - (diffRand.NextDouble() * n / 20.0) : 1.0 + (diffRand.NextDouble() * n / 10.0);
                klon.Attack = (int)Math.Round(klon.Attack * mod);
                klon.CurrentHp = klon.Hp;
                klon.WaterHealedThisRound = false;
                list.Add(klon);
            }
            return list;
        }
        private static int ResolveAttack(Kartya attacker, Kartya defender, out bool wasCrit, out bool dodged)
        {
            dodged = defender.TryDodge();
            wasCrit = false;
            if (dodged) return 0;

            int baseAttack = attacker.CalculateAttackDamage(out wasCrit);
            double mertek = TamadasArany(attacker, defender);
            int sebzesWithAdvantage = (int)(baseAttack * mertek);
            int vegsoSebzes = defender.CalculateIncomingDamage(sebzesWithAdvantage);
            return vegsoSebzes;
        }
        private static void AppendKijatszikLog(int kor, string who, Kartya card, int unused, bool doubleLine)
        {
            Naplo += $"{kor}.kor;{who};kijatszik;{card.Nev};{card.Attack};{card.CurrentHp};{card.Tipus()}\n";
            if (doubleLine) Naplo += "\n";
        }

        public static double TamadasArany(Kartya tamado, Kartya vedo)
        {
            if (tamado.Kartyatipus == vedo.Kartyatipus) return 1;

            switch (tamado.Kartyatipus)
            {
                case KartyaTipusok.Fire:
                    if (vedo.Kartyatipus == KartyaTipusok.Earth || vedo.Kartyatipus == KartyaTipusok.Water) return 2;
                    if (vedo.Kartyatipus == KartyaTipusok.Air) return 0.5;
                    break;
                case KartyaTipusok.Water:
                    if (vedo.Kartyatipus == KartyaTipusok.Fire || vedo.Kartyatipus == KartyaTipusok.Air) return 2;
                    if (vedo.Kartyatipus == KartyaTipusok.Earth) return 0.5;
                    break;
                case KartyaTipusok.Earth:
                    if (vedo.Kartyatipus == KartyaTipusok.Air || vedo.Kartyatipus == KartyaTipusok.Fire) return 2;
                    if (vedo.Kartyatipus == KartyaTipusok.Air) return 0.5;
                    break;
                case KartyaTipusok.Air:
                    if (vedo.Kartyatipus == KartyaTipusok.Earth || vedo.Kartyatipus == KartyaTipusok.Water) return 2;
                    if (vedo.Kartyatipus == KartyaTipusok.Fire) return 0.5;
                    break;
            }

            return 1;
        }

        public string Tipus()
        {
            switch (this.KTipus)
            {
                case KTipusok.Egyszeru:
                    return "egyszeru";
                case KTipusok.Kis:
                    return "kis";
                case KTipusok.Nagy:
                    return "nagy";
            }
            return "";
        }

        public string GetSimaKartyakKiiras()
        {
            List<string> nevek = new List<string>();
            foreach (Kartya k in this.KKartyak)
            {
                if (k.SimaKartya) nevek.Add(k.Nev);
            }
            return string.Join(",", nevek);
        }

        public string GetVezerNeve()
        {
            foreach (Kartya k in this.KKartyak)
            {
                if (!k.SimaKartya) return k.Nev;
            }
            return "";
        }

        public string GetBonuszKiiras()
        {
            switch (this.bonusz)
            {
                case Bonuszok.Sebzes:
                    return "sebzes";
                case Bonuszok.Eletero:
                    return "eletero";
                case Bonuszok.UjKartya:
                    return this.BonuszKartyaNev;
                default:
                    break;
            }
            return "";
        }

        public bool IsComplete()
        {
            (int maxSima, int maxVezer) = GetKazamataLimits();
            int currentSima = KKartyak.Count(k => k.SimaKartya);
            int currentVezer = KKartyak.Count(k => !k.SimaKartya);
            return currentSima == maxSima && currentVezer == maxVezer;
        }

        public (int maxSima, int maxVezer) GetKazamataLimits()
        {
            switch (KTipus)
            {
                case KTipusok.Egyszeru: return (1, 0);
                case KTipusok.Kis: return (3, 1);
                case KTipusok.Nagy: return (5, 1);
                default: return (0, 0);
            }
        }

        static void PrintHarc(string naplo, string file)
        {
            if (string.IsNullOrEmpty(Vilag.EleresiUtvonal)) return;
            try
            {
                string fullPath = Path.Combine(Vilag.EleresiUtvonal, file);
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                StreamWriter sw = new StreamWriter(fullPath);
                sw.Write(naplo);
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba a napló írásakor: {ex.Message}");
            }
        }
    }

}
