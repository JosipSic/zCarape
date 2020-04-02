using System;

namespace zCarape.Core.Models
{
    public class Predajnica
    {
        public long ID { get; set; }
        public long RadniNalogID { get; set; }
        public long MasinaID { get; set; }
        public DateTime Datum { get; set; }
        public long LiceID { get; set; }
        public byte Smena { get; set; }
        public long Kolicina { get; set; }
        public long DrugaKl { get; set; }
        public DateTime VremeUnosa { get; set; }
    }
}