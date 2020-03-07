using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using zCarape.Core;

namespace zCarape
{
    internal static class StartUpCode
    {

        public static void StartUp()
        {
            // TODO podesiti kulturu
            // TODO kreiraj rezervnu kopiju kod prvog pokretanja u danu

            // Provera da li postoji upisana putenja do baze podataka, ukoliko ne postoji trazim je u startnom direktorijumu
            if (string.IsNullOrWhiteSpace(GlobalniKod.BazaPath))
            {
                string startupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                GlobalniKod.BazaPath = Path.Combine(startupPath, "zBaza.db");
            }

            // Provera da li postoji upisana putenja do baze foldera sa fotografija, ukoliko ne postoji trazim je u startnom direktorijumu
            if (string.IsNullOrWhiteSpace(GlobalniKod.SlikeDir))
            {
                string startupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                GlobalniKod.BazaPath = Path.Combine(startupPath, "Slike");
            }

        }
    }
}
