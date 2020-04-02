using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Business
{
    public class MasinaURadu
    {
        public long MasinaID { get; set; }
        public string MasinaNaziv { get; set; }
        public List<Zadatak> Zadaci {get;set;}

    }
}
