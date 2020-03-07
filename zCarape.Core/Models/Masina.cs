using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Models
{
    public class Masina
    {
        public long ID { get; set; }
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public string Slika { get; set; }
        public bool Aktivan { get; set; }
        public DateTime VremeUnosa { get; set; }
    }
}
