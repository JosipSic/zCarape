using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace zCarape.Core.Business
{
    public class Zadatak
    {
        private long _prvaSmena1kl;
        private long _prvaSmena2kl;
        private long _prvaSmenaRadnikID;
        private long _drugaSmena1kl;
        private long _drugaSmena2kl;
        private long _drugaSmenaRadnikID;
        private long _trecaSmena1kl;
        private long _trecaSmena2kl;
        private long _trecaSmenaRadnikID;

        public byte StatusMasine { get; set; }
        public int Redosled { get; set; }
        public NalogURadu NalogURadu { get; set; }

        // Prva smena
        public long PrvaSmenaPredajnicaID { get; set; }

        public long PrvaSmena1kl
        {
            get => _prvaSmena1kl; 
            set
            {
                _prvaSmena1kl = value;
            }
        }
        public long PrvaSmena2kl
        {
            get => _prvaSmena2kl;
            set
            {
                _prvaSmena2kl = value;
            }
        }
        public long PrvaSmenaRadnikID
        {
            get => _prvaSmenaRadnikID;
            set
            {
                _prvaSmenaRadnikID = value;
            }
        }

        // Druga smena
        public long DrugaSmenaPredajnicaID { get; set; }
        public long DrugaSmena1kl
        {
            get => _drugaSmena1kl; 
            set { _drugaSmena1kl = value; }
        }
        public long DrugaSmena2kl { 
            get => _drugaSmena2kl; 
            set => _drugaSmena2kl = value; 
        }
        public long DrugaSmenaRadnikID { 
            get => _drugaSmenaRadnikID; 
            set => _drugaSmenaRadnikID = value; 
        }

        // Treca smena
        public long TrecaSmenaPredajnicaID { get; set; }
        public long TrecaSmena1kl { 
            get => _trecaSmena1kl;
            set { _trecaSmena1kl = value; }
        }
        public long TrecaSmena2kl { 
            get => _trecaSmena2kl;
            set { _trecaSmena2kl = value; }
        }
        public long TrecaSmenaRadnikID { 
            get => _trecaSmenaRadnikID;
            set { _trecaSmenaRadnikID = value; }
        }

    }
}
