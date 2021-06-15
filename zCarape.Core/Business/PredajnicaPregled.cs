using System;
using System.Collections.Generic;
using System.Text;
using zCarape.Core.Models;

namespace zCarape.Core.Business
{
    public class PredajnicaPregled:Predajnica
    {
        public string MasinaNaziv { get; set; }
        public string LiceIme { get; set; }
        public string LicePrezime { get; set; }
        public string LicePrezimeIme { get; set; }
    }
}
