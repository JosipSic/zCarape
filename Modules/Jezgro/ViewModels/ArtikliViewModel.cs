using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Jezgro.ViewModels
{
    public class ArtikliViewModel : BindableBase
    {
        #region Fields
        private long loadedID;
        private readonly IDbService _dbService;
        #endregion //Fields ---------------------------------------------------------

        #region Properties
        public string Sifra { get; set; }
        public string Naziv { get; set; }
        public string Jm { get; set; }
        public string Slika { get; set; }
        public string BarKod { get; set; }
        public DateTime VremeUnosa { get; set; }


        // Artikli
        private ObservableCollection<Artikal> _artikli;
        public ObservableCollection<Artikal> Artikli
        {
            get { return _artikli; }
            set { SetProperty(ref _artikli, value); }
        }

        // Velicine
        private ObservableCollection<Velicina> _velicine;
        public ObservableCollection<Velicina> Velicine
        {
            get { return _velicine; }
            set { SetProperty(ref _velicine, value); }
        }

        // Dezeni
        private ObservableCollection<DezenArtikla> _dezeni;
        public ObservableCollection<DezenArtikla> Dezeni
        {
            get { return _dezeni; }
            set { SetProperty(ref _dezeni, value); }
        }

        // SelectedArtikal
        private Artikal _selectedArtikal;
        public Artikal SelectedArtikal
        {
            get { return _selectedArtikal; }
            set {
                if (_selectedArtikal!=value)
                {
                    SetProperty(ref _selectedArtikal, value);
                    PopuniSvojstvaZaSelektovaniArtikal();
                    PopuniVelicineSelektovanogArtikla();
                    PopuniDezeneSelektovanogArtikla();
                }
            }
        }
        #endregion //Properties ------------------------------------------------------

        #region Cmd

        // Snimi
        private DelegateCommand _snimiCommand;
        public DelegateCommand SnimiCommand =>
            _snimiCommand ?? (_snimiCommand = new DelegateCommand(ExecuteSnimiCommand, UslovZaSnimanje));

        void ExecuteSnimiCommand()
        {
            if (!UslovZaSnimanje()) return;
            Artikal azuriranArtikal = new Artikal() { ID = this.loadedID, Sifra = this.Sifra, Naziv = this.Naziv, Jm = this.Jm, Slika = this.Slika, BarKod = this.BarKod };
            long odgovor = _dbService.InsertOrUpdateArtikal(azuriranArtikal);
            if (odgovor>0)
            {
                if (loadedID==0)
                {
                    azuriranArtikal.ID = odgovor;
                    this.Artikli.Add(azuriranArtikal);
                }
               SelectedArtikal = azuriranArtikal;
            }
        }

        private bool UslovZaSnimanje()
        {
            if (string.IsNullOrWhiteSpace(this.Sifra)) return false;
            if (string.IsNullOrWhiteSpace(this.Naziv)) return false;
            return true;
        }

        // Odustani
        private DelegateCommand _odustaniCommand;
        public DelegateCommand OdustaniCommand =>
            _odustaniCommand ?? (_odustaniCommand = new DelegateCommand(ExecuteOdustaniCommand, CanExecuteOdustaniCommand));

        void ExecuteOdustaniCommand()
        {
            PopuniSvojstvaZaSelektovaniArtikal();
        }

        bool CanExecuteOdustaniCommand()
        {
            return true;
        }

        #endregion //Cmd

        #region Methods

        private void FormirajSpisakArtikala()
        {
            Artikli = new ObservableCollection<Artikal>(_dbService.GetAllArtikli());
        }

        // Kada se promeni selektovani artikal 
        private void PopuniSvojstvaZaSelektovaniArtikal()
        {
            this.loadedID = SelectedArtikal.ID;
            this.Sifra = SelectedArtikal.Sifra;
            this.Naziv = SelectedArtikal.Naziv;
            this.Jm = SelectedArtikal.Jm;
            this.Slika = SelectedArtikal.Slika;
            this.BarKod = SelectedArtikal.BarKod;
            this.VremeUnosa = SelectedArtikal.VremeUnosa;
        }

        private void PopuniVelicineSelektovanogArtikla()
        {
            if (this.loadedID==0)
            {
                this.Velicine = new ObservableCollection<Velicina>();
            }
            else
            {
                this.Velicine = new ObservableCollection<Velicina>(_dbService.GetVelicineArtikla(this.loadedID));
            }
        }

        private void PopuniDezeneSelektovanogArtikla()
        {
            if (this.loadedID == 0)
            {
                this.Dezeni = new ObservableCollection<DezenArtikla>();
            }
            else
            {
                this.Dezeni = new ObservableCollection<DezenArtikla>(_dbService.GetDezeniArtikla(this.loadedID));
            }
        }

        private void Blanko()
        {
            this.SelectedArtikal = new Artikal();
        }

        #endregion //Methods

        #region Ctor

        public ArtikliViewModel(IDbService dbService)
        {
            _dbService = dbService;
            FormirajSpisakArtikala();
        }

        #endregion //Ctor
    }
}
