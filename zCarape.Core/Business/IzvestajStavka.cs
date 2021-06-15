using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Business
{
    public class IzvestajStavka
    {
        public string Sifra { get; set; }
        public string Naziv { get; set; }
        public string Velicina { get; set; }
        public string Dezen { get; set; }
        public long RadniNalog { get; set; }
        public string Masina { get; set; }
        public long Smena { get; set; }
        public long Kolicina { get; set; }
        public long DrugaKl { get; set; }
        public DateTime Datum { get; set; }
        public string Radnik { get; set; }
        public string Mesec { get; set; }
        public string Godina { get; set; }
    }
}
