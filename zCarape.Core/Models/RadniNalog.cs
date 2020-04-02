using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Models
{
    public class RadniNalog
    {
        public long ID { get; set; }
        public long ArtikalID { get; set; }
        public long DezenArtiklaID { get; set; }
        public long VelicinaID { get; set; }
        public long Cilj { get; set; }
        public long Napravljeno { get; set; }
        public string Opis { get; set; }
        public string Podsetnik { get; set; }
        public bool Hitno { get; set; }
        public byte Status { get; set; }
        public DateTime VremeUnosa { get; set; }
    }
}