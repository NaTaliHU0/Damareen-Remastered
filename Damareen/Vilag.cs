using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Damareen
{
    internal class Vilag
    {
        public static List<Kartya> kartyak = new List<Kartya>();
        public static List<Kazamata> kazamatak = new List<Kazamata>();
        public static string EleresiUtvonal;
        public static string VilagFajlNev;
        public static int NehezsegiSzint { get; set; } = 0;

        public static void ExportVilag(string file)
        {
            if (string.IsNullOrEmpty(EleresiUtvonal)) throw new Exception("Nincs beállítva elérési útvonal a világ mentéséhez.");
            if (string.IsNullOrEmpty(file)) throw new Exception("Nincs beállítva világfájlnév a mentéshez.");

            try
            {
                Directory.CreateDirectory(EleresiUtvonal);
                StreamWriter sw = new StreamWriter(Path.Combine(EleresiUtvonal, file));

                foreach (Kartya k in kartyak)
                {
                    if (k.SimaKartya)
                    {
                        k.UpdateArrayData();
                        sw.WriteLine($"kartya;{k.Nev};{k.Attack};{k.Hp};{k.Tipus()}");
                    }
                    else
                    {
                        sw.WriteLine($"\nvezer;{k.Nev};{k.Attack};{k.Hp};{k.Tipus()}");
                    }
                }
                sw.WriteLine();

                foreach (Kazamata k in kazamatak)
                {
                    string sor = $"kazamata;{k.Tipus()};{k.KNev};{k.GetSimaKartyakKiiras()}";
                    if (k.KTipus == KTipusok.Egyszeru)
                    {
                        sor += $";{k.GetBonuszKiiras()}";
                    }
                    else if (k.KTipus == KTipusok.Kis)
                    {
                        sor += $";{k.GetVezerNeve()};{k.GetBonuszKiiras()}";
                    }
                    else if (k.KTipus == KTipusok.Nagy)
                    {
                        sor += $";{k.GetVezerNeve()}";
                    }
                    sw.WriteLine(sor);
                }

                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba a világ exportálásakor: {ex.Message}");
            }
        }
    }
}