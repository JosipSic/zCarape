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

        public long ID { get; set; }
        public long MasinaID { get; set; }
        public byte StatusMasine { get; set; }
        public bool Hitno { get; set; }
        public int Redosled { get; set; }
        public NalogURadu NalogURadu { get; set; }

        private bool _canGoLeft;
        public bool CanGoLeft
        {
            get { return _canGoLeft; }
            set { _canGoLeft = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime _datumPredajnice;
        public DateTime DatumPredajnice
        {
            get => _datumPredajnice;
            set
            {
                _datumPredajnice = value;
                NotifyPropertyChanged();
            }
        }

        // Prva smena
        public long PrvaSmenaPredajnicaID { get; set; }

        private long _prvaSmena1kl;
        public long PrvaSmena1kl
        {
            get => _prvaSmena1kl;
            set
            {
                _prvaSmena1kl = value;
                NotifyPropertyChanged();
            }
        }

        private long _prvaSmena2kl;
        public long PrvaSmena2kl
        {
            get => _prvaSmena2kl; set
            {
                _prvaSmena2kl = value;
                NotifyPropertyChanged();
            }
        }

        private long _prvaSmenaRadnikID;
        public long PrvaSmenaRadnikID
        {
            get => _prvaSmenaRadnikID;
            set
            {
                _prvaSmenaRadnikID = value;
                NotifyPropertyChanged();
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

        private long _drugaSmena1kl;
        public long DrugaSmena1kl
        {
            get => _drugaSmena1kl; set
            {
                _drugaSmena1kl = value;
                NotifyPropertyChanged();
            }
        }

        private long _drugaSmena2kl;
        public long DrugaSmena2kl
        {
            get => _drugaSmena2kl; set
            {
                _drugaSmena2kl = value;
                NotifyPropertyChanged();
            }
        }

        private long _drugaSmenaRadnikID;
        public long DrugaSmenaRadnikID
        {
            get => _drugaSmenaRadnikID;
            set
            {
                _drugaSmenaRadnikID = value;
                NotifyPropertyChanged();
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

        private long _trecaSmena1kl;
        public long TrecaSmena1kl
        {
            get => _trecaSmena1kl; set
            {
                _trecaSmena1kl = value;
                NotifyPropertyChanged();
            }
        }
        private long _trecaSmena2kl;
        public long TrecaSmena2kl
        {
            get => _trecaSmena2kl; set
            {
                _trecaSmena2kl = value;
                NotifyPropertyChanged();
            }
        }

        private long _trecaSmenaRadnikID;
        public long TrecaSmenaRadnikID
        {
            get => _trecaSmenaRadnikID;
            set
            {
                _trecaSmenaRadnikID = value;
                NotifyPropertyChanged();
                IsTrecaSmena = value > 0;
            }
        }

        private bool _isTrecaSmena;
        public bool IsTrecaSmena
        {
            get { return _isTrecaSmena; }
            set
            {
                _isTrecaSmena = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isInFocus;
        public bool IsInFocus
        {
            get { return _isInFocus; }
            set { _isInFocus = value;
                NotifyPropertyChanged();
            }
        }

        // Ako je isti radni nalog u fokusu na drugoj masini
        private bool _isAnotherInFocus;
        public bool IsAnotherInFocus
        {
            get { return _isAnotherInFocus; }
            set
            {
                _isAnotherInFocus = value;
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
