using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Models
{
    public class DezenArtikla
    {
        public long ID { get; set; }
        public long ArtikalID { get; set; }
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public string Slika1 { get; set; }
        public string Slika2 { get; set; }
        public string Slika3 { get; set; }
        public bool Aktivan { get; set; }
        public DateTime VremeUnosa { get; set; }

    }
}
