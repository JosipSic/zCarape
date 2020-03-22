using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using zCarape.Core;

namespace zCarape
{
    internal static class StartUpCode
    {

        public static void StartUp()
        {
            // TODO podesiti kulturu

            // Provera da li postoji upisana putenja do baze podataka, ukoliko ne postoji trazim je u startnom direktorijumu
            if (string.IsNullOrWhiteSpace(GlobalniKod.BazaPath))
            {
                string startupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                GlobalniKod.BazaPath = Path.Combine(startupPath, "zBaza.db");
            }


            // TODO: Konekcija sa bazom: TREBA PROMENITI DA SE METOD POZIVA PREKO INTERFEJSA
            if (!(new zCarape.Services.DBService()).TestConnection(out string poruka))
            {
                MessageBox.Show(poruka, "Nije moguce uspostaviti konekciju sa bazom podataka", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.Current.Shutdown();
            }


            // Provera da li postoji upisana putenja do baze foldera sa fotografija, ukoliko ne postoji trazim je u startnom direktorijumu
            if (string.IsNullOrWhiteSpace(GlobalniKod.SlikeDir))
            {
                string startupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                GlobalniKod.BazaPath = Path.Combine(startupPath, "Slike");
            }

            // Kreiranje rezervne kopije
            try
            {
                KreirajRezervnuKopiju();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private static void KreirajRezervnuKopiju()
        {
            string arhivaDir = ConfigurationManager.AppSettings["ArhivaDir"];
            if (string.IsNullOrWhiteSpace(arhivaDir))
            {
                MessageBox.Show("U konfiguracionom fajlu nije podesena lokacija za automatsko formiranje rezirvnih kopija");
                return;
            }

            // Da li destinacija postoji
            if (!Directory.Exists(arhivaDir))
            {
                try
                { Directory.CreateDirectory(arhivaDir); }
                catch
                {
                    MessageBox.Show($"Nije moguce kreirati direktorijum \"{arhivaDir}\" za kreiranje rezervnih kopija");
                    return;
                }
            }

            string nazivKopije = Path.GetFileNameWithoutExtension(GlobalniKod.BazaPath) + "_" + DateTime.Now.ToString("yyy-MM-dd")
                + ".zip";

            string punaPutanjaKopije = Path.Combine(arhivaDir, nazivKopije);

            if (File.Exists(punaPutanjaKopije)) return;


            using FileStream fileStream = new FileStream(punaPutanjaKopije, FileMode.CreateNew);
            using ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Update);
            zipArchive.CreateEntryFromFile(GlobalniKod.BazaPath, Path.GetFileName(GlobalniKod.BazaPath));
            zipArchive.Dispose();
        }

    }
}
