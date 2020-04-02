using System.Collections.Generic;

namespace zCarape.Core.Business
{
    public class NalogURadu
    {
        public long RadniNalogID { get; set; }
        public long ArtikalID { get; set; }
        public string ArtikalSifra { get; set; }
        public string ArtikalNaziv { get; set; }
        public string ArtikalVelicina { get; set; }
        public string ArtikalDezen { get; set; }
        public string Slika1 { get; set; }
        public string Slika2 { get; set; }
        public string Slika3 { get; set; }
        public string PutanjaFajla { get; set; }
        public long Cilj { get; set; }
        public bool Hitno { get; set; }
        public byte StatusNaloga { get; set; }

    }
}