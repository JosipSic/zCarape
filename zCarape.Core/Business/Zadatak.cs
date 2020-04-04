using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace zCarape.Core.Business
{
    public class Zadatak : ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public long MasinaID { get; set; }
        public byte StatusMasine { get; set; }
        public int Redosled { get; set; }
        public NalogURadu NalogURadu { get; set; }
        public DateTime DatumPredajnice { get; set; }

        // Prva smena
        public long PrvaSmenaPredajnicaID { get; set; }
        public long PrvaSmena1kl { get; set; }
        public long PrvaSmena2kl { get; set; }

        private long _prvaSmenaRadnikID;
        public long PrvaSmenaRadnikID
        {
            get => _prvaSmenaRadnikID;
            set
            {
                _prvaSmenaRadnikID = value;
                IsPrvaSmena = value > 0;
            }
        }

        private bool _isPrvaSmena;

        public bool IsPrvaSmena
        {
            get { return _isPrvaSmena; }
            set
            {
                _isPrvaSmena = value;
                NotifyPropertyChanged();
            }
        }

        // Druga smena
        public long DrugaSmenaPredajnicaID { get; set; }
        public long DrugaSmena1kl { get; set; }
        public long DrugaSmena2kl { get; set; }

        private long _drugaSmenaRadnikID;
        public long DrugaSmenaRadnikID
        {
            get => _drugaSmenaRadnikID;
            set
            {
                _drugaSmenaRadnikID = value;
                IsDrugaSmena = value > 0;
            }
        }

        private bool _isDrugaSmena;
        public bool IsDrugaSmena
        {
            get { return _isDrugaSmena; }
            set
            {
                _isDrugaSmena = value;
                NotifyPropertyChanged();
            }
        }


        // Treca smena
        public long TrecaSmenaPredajnicaID { get; set; }
        public long TrecaSmena1kl { get; set; }
        public long TrecaSmena2kl { get; set; }

        private long _trecaSmenaRadnikID;
        public long TrecaSmenaRadnikID
        {
            get => _trecaSmenaRadnikID;
            set { _trecaSmenaRadnikID = value; }
        }

        private bool _isTrecaSmena;

        public bool IsTrecaSmena
        {
            get { return _isTrecaSmena; }
            set { _isTrecaSmena = value;
                NotifyPropertyChanged();
            }
        }


        /// <summary>
        /// Shallow Copy. NalogURadu points to the same instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

}
