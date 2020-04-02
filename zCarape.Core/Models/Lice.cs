using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Models
{
    public class Lice
    {
        public long ID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string RadnoMesto { get; set; }
        public bool Aktivan { get; set; }
        public DateTime VremeUnosa { get; set; }
    }
}
