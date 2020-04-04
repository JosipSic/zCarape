using System.Collections.Generic;
using System.ComponentModel;

namespace zCarape.Core.Business
{
    public class NalogURadu: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public long RadniNalogID { get; set; }
        public long ArtikalID { get; set; }
        public string ArtikalSifra { get; set; }
        public string ArtikalNaziv { get; set; }
        public string ArtikalVelicina { get; set; }
        public string ArtikalDezen { get; set; }
        public string Slika1 { get; set; }
        public string Slika2 { get; set; }
        public string Slika3 { get; set; }
        public string PutanjaFajla { get; set; }
        public long Cilj { get; set; }
        public bool Hitno { get; set; }
        public byte StatusNaloga { get; set; }

        // Uradjeno
        private long _uradjeno;
        public long Uradjeno
        {
            get { return _uradjeno; }
            set { _uradjeno = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Uradjeno")); 
            }
        }

        // Fali
        private long _fali;
        public long Fali
        {
            get { return _fali; }
            set { _fali = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Fali"));
            }
        }

    }
}