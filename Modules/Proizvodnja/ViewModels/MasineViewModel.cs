using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class MasineViewModel : BindableBase
    {
        #region Fields

        private readonly IDbService _dbService;
        private long id = 0;

        #endregion //Fields

        #region Properties

        private string _naziv;
        public string Naziv
        {
            get { return _naziv; }
            set { SetProperty(ref _naziv, value); }
        }

        private string _opis;
        public string Opis
        {
            get { return _opis; }
            set { SetProperty(ref _opis, value); }
        }

        private string _slika;
        public string Slika
        {
            get { return _slika; }
            set { SetProperty(ref _slika, value); }
        }

        private bool _aktivan;
        public bool Aktivan
        {
            get { return _aktivan; }
            set { SetProperty(ref _aktivan, value); }
        }

        private ObservableCollection<Masina> _masine;
        public ObservableCollection<Masina> Masine
        {
            get { return _masine; }
            set { SetProperty(ref _masine, value); }
        }

        #endregion //Properties

        #region Cmd

        // Snimi
        private DelegateCommand _snimiCommand;
        public DelegateCommand SnimiCommand =>
            _snimiCommand ?? (_snimiCommand = new DelegateCommand(ExecuteSnimiCommand));

        void ExecuteSnimiCommand()
        {
            if (String.IsNullOrWhiteSpace(Naziv)) return;

            long odgovor = _dbService.InsertOrUpdateMasina(new Masina() { ID = this.id, Naziv=this.Naziv, Opis = this.Opis, Slika=this.Slika, Aktivan=this.Aktivan });
            if (odgovor == 0)
            {

            }
            else
            {
                AzurirajListu(odgovor);
                BlankoForma();
            }
        }

        // Izbrisi
        private DelegateCommand _izbrisiCommand;
        public DelegateCommand IzbrisiCommand =>
            _izbrisiCommand ?? (_izbrisiCommand = new DelegateCommand(ExecuteIzbrisiCommand));

        void ExecuteIzbrisiCommand()
        {
            if (this.id == 0) return;

            if (_dbService.MasinaImaZavisneZapise(id: this.id))
            {
                MessageBox.Show("Postoje zavisni zapisi. Ukoliko se mašina više ne koristi obeležite da je neaktivna","Nije dozvoljeno brisanje", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_dbService.DeleteMasina(id: this.id))
            {
                Masina izbrisanaMasina = Masine.First((m) => m.ID == this.id);
                if (izbrisanaMasina != null)
                {
                    this.Masine.Remove(izbrisanaMasina);
                    this.BlankoForma();
                }
            }
        }

        // Odustani
        private DelegateCommand _odustaniCommand;

        public DelegateCommand OdustaniCommand =>
            _odustaniCommand ?? (_odustaniCommand = new DelegateCommand(ExecuteOdustaniCommand));


        void ExecuteOdustaniCommand()
        {
            this.BlankoForma();
        }

        #endregion //Cmd

        #region Methods

        private void BlankoForma()
        {
            this.id = 0;
            NoviUnosPriprema();
        }

        private void NoviUnosPriprema()
        {
            this.Naziv = this.Opis = this.Slika = string.Empty;
            this.Aktivan = true;

        }

        private bool PromenjenJeNaziv(string value)
        {
            Masina masina = _dbService.GetByNazivMasina(value);
            if (masina == null)
            {
                // Ne postoji
                NoviUnosPriprema();
            }
            else
            {
                // Rezim izmene
                this.id = masina.ID;
                this.Naziv = masina.Naziv;
                this.Opis = masina.Opis;
                this.Slika = masina.Slika;
                this.Aktivan = masina.Aktivan;
            }
            return true;
        }


        private void FormirajSpisakMasina()
        {
            Masine = new ObservableCollection<Masina>(_dbService.GetAllMasine());
        }

        private void AzurirajListu(long odgovor)
        {
            if (this.id == 0)
            {
                Masine.Add(new Masina()
                {
                    ID = (int)odgovor,
                    Naziv = this.Naziv,
                    Opis = this.Opis,
                    Slika = this.Slika,
                    Aktivan = this.Aktivan,
                });
            }
            else
            {
                Masina masina = this.Masine.First((p) => p.ID == this.id);
                if (masina != null)
                {
                    masina.Naziv = this.Naziv;
                    masina.Opis = this.Opis;
                    masina.Slika = this.Slika;
                    masina.Aktivan = this.Aktivan;
                }
            }
        }


        #endregion //Methods

        #region Ctor

        public MasineViewModel(IDbService dbService)
        {
            _dbService = dbService;

            FormirajSpisakMasina();
        }

        #endregion //Ctor
    }
}
