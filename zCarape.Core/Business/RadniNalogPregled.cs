using System;
using System.Collections.Generic;
using System.Text;
using zCarape.Core.Models;

namespace zCarape.Core.Business
{
    public class RadniNalogPregled: RadniNalog
    {
        public string ArtikalSifra { get; set; }
        public string ArtikalNaziv { get; set; }
        public string DezenNaziv { get; set; }
        public string DezenOpis { get; set; }
        public string DezenPutanja { get; set; }
        public string Slika1 { get; set; }
        public string Slika2 { get; set; }
        public string Slika3 { get; set; }

        public string VelicinaOznaka { get; set; }

        public List<PredajnicaPregled> Predajnice { get; set; } = new List<PredajnicaPregled>();
    }
}
