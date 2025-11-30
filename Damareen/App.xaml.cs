using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace Damareen
{
    public partial class App : Application
    {
        public static bool UIFlag { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            AttachConsole(-1);
            UIFlag = e.Args.Contains("--ui");
            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                Console.WriteLine("Használat: Damareen.exe [--ui | <test_dir_path>]");
                Shutdown(1);
                return;
            }

            if (e.Args[0] == "--ui")
            {
                return;
            }

            try
            {
                RunAutomatedTest(e.Args[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Shutdown();
            }
        }

        private void RunAutomatedTest(string v)
        {
            Vilag.EleresiUtvonal = v + "/";
            foreach (string line in File.ReadAllLines($"{Vilag.EleresiUtvonal}in.txt"))
            {
                try
                {
                    string[] data = line.Split(';');
                    if (data.Length > 0 && data[0] != "")
                    {
                        switch (data[0].Trim())
                        {
                            case "uj kartya":
                            case "uj vezer":
                                new Kartya(data, true);
                                break;
                            case "uj kazamata":
                                new Kazamata(data);
                                break;
                            case "felvetel gyujtemenybe":
                                Jatekos.FelvetelGyujtemeny(data);
                                break;
                            case "uj pakli":
                                Jatekos.UjPakli(data);
                                break;
                            case "uj jatekos":
                                Jatekos.UjJatekos = true;
                                break;
                            case "harc":
                                Kazamata.UjHarc(data);
                                break;
                            case "export vilag":
                                Vilag.ExportVilag(data[1]);
                                break;
                            case "export jatekos":
                                Jatekos.ExportJatekos(data[1]);
                                break;
                            default:
                                Console.WriteLine($"Nincs ilyen parancs: {data[0]}");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern bool AttachConsole(int processId);
    }
}