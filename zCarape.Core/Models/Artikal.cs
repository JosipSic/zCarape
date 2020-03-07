using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Models
{
    public class Artikal
    {
        public long ID { get; set; }
        public string Sifra { get; set; }
        public string Naziv { get; set; }
        public string Jm { get; set; }
        public string Slika { get; set; }
        public string BarKod { get; set; }
        public DateTime VremeUnosa { get; set; }

        public List<Velicina> Velicine { get; set; } = new List<Velicina>();
        public List<DezenArtikla> Dezeni { get; set; } = new List<DezenArtikla>();

    }
}
