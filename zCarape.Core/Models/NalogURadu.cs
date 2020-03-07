using System.Collections.Generic;

namespace zCarape.Core.Models
{
    public class NalogURadu
    {
        public long RadniNalogID { get; set; }
        public long ArtikalID { get; set; }
        public string ArtikalNaziv { get; set; }
        public string ArtikalVelicina { get; set; }
        public string ArtikalDezen { get; set; }
        public string Slika { get; set; }
        public string PutanjaFajla { get; set; }
        public int Cilj { get; set; }

        public List<MasinaURadu> MasineURadu { get; set; }
        public List<Predajnica> Predajnice { get; set; }
    }
}