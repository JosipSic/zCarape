using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace zCarape.Core.Business
{
    public class MasinaURadu : INotifyPropertyChanged
    {
        private Istorija _istorija;

        public long MasinaID { get; set; }
        public string MasinaNaziv { get; set; }
        public Istorija Istorija
        {
            get => _istorija; 
            set
            {
                _istorija = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Istorija"));
            }
        }
        public List<Zadatak> Zadaci { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
