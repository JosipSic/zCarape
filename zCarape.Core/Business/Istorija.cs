using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace zCarape.Core.Business
{
    public class Istorija: INotifyPropertyChanged
    {
		public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		// RadniNalogID
		private long _radniNalogID;
		public long RadniNalogID
		{
			get { return _radniNalogID; }
			set
			{
				_radniNalogID = value;
				//NotifyPropertyChanged();
			}
		}

		// ArtikalNaziv
		private string _artikalNaziv;
		public string ArtikalNaziv
		{
			get { return _artikalNaziv; }
			set
			{
				_artikalNaziv = value;
				//NotifyPropertyChanged();
			}
		}

		// ArtikalVelicina
		private string _artikalVelicina;
		public string ArtikalVelicina
		{
			get { return _artikalVelicina; }
			set
			{
				_artikalVelicina = value;
				//NotifyPropertyChanged();
			}
		}

		// ArtikalDezen
		private string _artikalDezen;
		public string ArtikalDezen
		{
			get { return _artikalDezen; }
			set
			{
				_artikalDezen = value;
				//NotifyPropertyChanged();
			}
		}

		public string Slika1 { get; set; }
		public DateTime Datum { get; set; }

		// Kolicina (po zadnjoj predajnici)
		private long _kolicina;
		public long Kolicina
		{
			get { return _kolicina; }
			set {
				_kolicina = value;
				//NotifyPropertyChanged();
			}
		}

		// Smena (po zadnjoj predajnici)
		private byte _smena;
		public byte Smena
		{
			get { return _smena; }
			set { _smena = value;
				//NotifyPropertyChanged();
			}
		}

		// DrugaKl (po zadnjoj predajnici)
		private long _drugaKl;
		public long DrugaKl
		{
			get { return _drugaKl; }
			set { _drugaKl = value;
				//NotifyPropertyChanged();
			}
		}

		// LiceID (po zadnjoj predajnici)
		public string Radnik { get; set; }


		// IsNotZadnji
		private bool _isNotZadnji;
		public bool IsNotZadnji
		{
			get { return _isNotZadnji; }
			set { _isNotZadnji = value;
				//NotifyPropertyChanged();
			}
		}

		// IsNotPrvi
		public bool IsNotPrvi { get; set; }

	}
}
