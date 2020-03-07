using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Models
{
    public class VelicinaArtikla
    {
        public long ID { get; set; }
        public long ArtikalID { get; set; }
        public long VelicinaID { get; set; }
        public DateTime VremeUnosa { get; set; }

    }
}