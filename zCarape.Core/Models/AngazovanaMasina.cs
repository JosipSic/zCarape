using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Models
{
    public class AngazovanaMasina
    {
        public long ID { get; set; }
        public long RadniNalogID { get; set; }
        public long MasinaID { get; set; }
        public byte Status { get; set; }
        public int Redosled { get; set; }
    }
}
