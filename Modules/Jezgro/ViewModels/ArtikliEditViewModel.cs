using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using zCarape.Core;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Jezgro.ViewModels
{
    public class ArtikliEditViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        enum AkcijaEnum
        {
            Odustani,
            Snimi,
            Izbrisi
        }

        #region Fields
        private readonly IDbService _dbService;
        private readonly IRegionManager _regionManager;
        private long _loadedID;
        private bool _keepAlive = true;
        #endregion

        #region Properties
        // Naslov
        private string _naslov = "Novi artikal";
        public string Naslov
        {
            get { return _naslov; }
            set { SetProperty(ref _naslov, value); }
        }

        // Sifra
        private string _sifra;
        public string Sifra
        {
            get { return _sifra; }
            set { SetProperty(ref _sifra, value); }
        }

        // Naziv
        private string _naziv;
        public string Naziv
        {
            get { return _naziv; }
            set { SetProperty(ref _naziv, value); }
        }

        // JM
        private string _jm;
        public string Jm
        {
            get { return _jm; }
            set { SetProperty(ref _jm, value); }
        }

        // Slika
        private string _slika;
        public string Slika
        {
            get { return _slika; }
            set { SetProperty(ref _slika, value);
                if (string.IsNullOrWhiteSpace(value))
                    PunaPutanjaDoSlike = string.Empty;
                else
                    PunaPutanjaDoSlike = Path.Combine(GlobalniKod.SlikeDir, value);
            }
        }

        // BarKod
        private string _barKod;
        public string BarKod
        {
            get { return _barKod; }
            set { SetProperty(ref _barKod, value); }
        }

        // NemaIzabranihVelicina
        private bool _nemaIzabranihVelicina;
        public bool NemaIzabranihVelicina
        {
            get { return _nemaIzabranihVelicina; }
            set { SetProperty(ref _nemaIzabranihVelicina, value); }
        }

        // VremeUnosa
        private DateTime _vremeUnosa;
        public DateTime VremeUnosa
        {
            get { return _vremeUnosa; }
            set { SetProperty(ref _vremeUnosa, value); }
        }

        // VelicineZaIzbor
        private ObservableCollection<VelicinaZaIzbor> _velicineZaIzbor;
        public ObservableCollection<VelicinaZaIzbor> VelicineZaIzbor
        {
            get { return _velicineZaIzbor; }
            set { SetProperty(ref _velicineZaIzbor, value); }
        }

        // PunaPutanjaDoSlike
        //public string PunaPutanjaDoSlike { get; set; }
        private string _punaPutanjaDoSlike;
        public string PunaPutanjaDoSlike
        {
            get { return _punaPutanjaDoSlike; }
            set { SetProperty(ref _punaPutanjaDoSlike, value); }
        }


        #endregion

        #region Commands

        // SnimiCommand
        private DelegateCommand _snimiCommand;
        public DelegateCommand SnimiCommand =>
            _snimiCommand ?? (_snimiCommand = new DelegateCommand(ExecuteSnimiCommand));

        void ExecuteSnimiCommand()
        {
            if (!UslovZaUpis())
                return;

            long odgovor = _dbService.InsertOrUpdateArtikal(new Artikal() { ID = this._loadedID, Sifra = this.Sifra, Naziv = this.Naziv, 
                Jm = this.Jm, BarKod = this.BarKod, Slika = this.Slika });
            if (odgovor>0)
            {
                _loadedID = odgovor;
                SnimiVelicine();
                NazadIUkloniViewIzMemorije(AkcijaEnum.Snimi);
            }
        }

        // OdustaniCommand
        private DelegateCommand _odustaniCommand;
        public DelegateCommand OdustaniCommand =>
            _odustaniCommand ?? (_odustaniCommand = new DelegateCommand(ExecuteOdustaniCommand));

        void ExecuteOdustaniCommand()
        {
            NazadIUkloniViewIzMemorije(AkcijaEnum.Odustani);
        }

        //Izbrisi Command
        private DelegateCommand _izbrisiCommand;
        public DelegateCommand IzbrisiCommand =>
            _izbrisiCommand ?? (_izbrisiCommand = new DelegateCommand(ExecuteIzbrisiCommand, CanExecuteIzbrisiCommand));

        private bool CanExecuteIzbrisiCommand()
        {
            return this._loadedID > 0;
        }

        void ExecuteIzbrisiCommand()
        {
            if (this._loadedID == 0)
            {
                return;
            }

            if (MessageBox.Show("Da li ste sigurni da želite da izbrišete ovaj artikal?\nBrisanjem artikal obrisaćete i sve njegove dezene!", "Brisanje", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                == MessageBoxResult.Yes)
            {
                if (_dbService.IzbrisiArtikal(this._loadedID))
                {
                    NazadIUkloniViewIzMemorije(AkcijaEnum.Izbrisi);
                }
            }
        }


        // KreirajNovuVelicinuCommand
        private DelegateCommand _kreirajNovuVelicinuCommand;
        public DelegateCommand KreirajNovuVelicinuCommand =>
            _kreirajNovuVelicinuCommand ?? (_kreirajNovuVelicinuCommand = new DelegateCommand(ExecuteKreirajNovuVelicinuCommand));

        void ExecuteKreirajNovuVelicinuCommand()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.Velicine);
        }

        // VelicinaCommand Kada korisnik stiklira ili odstiklira velicinu
        private DelegateCommand _velicinaCommand;
        public DelegateCommand VelicinaCommand =>
            _velicinaCommand ?? (_velicinaCommand = new DelegateCommand(ExecuteVelicinaCommand));

        void ExecuteVelicinaCommand()
        {
            NemaIzabranihVelicina = !VelicineZaIzbor.Any(v => v.Izbor);
        }

        // DodajSlikuCommand
        private DelegateCommand _dodajSlikuCommand;
        public DelegateCommand DodajSlikuCommand =>
            _dodajSlikuCommand ?? (_dodajSlikuCommand = new DelegateCommand(ExecuteDodajSlikuCommand));

        void ExecuteDodajSlikuCommand()
        {
            // 1-Dijalog za lociranje slike
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Slike (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = GlobalniKod.SlikeDir;
            string folder, imeSlike;
            if (openFileDialog.ShowDialog() == true)
            {
                 folder = Path.GetDirectoryName(openFileDialog.FileName);
                 imeSlike = Path.GetFileName(openFileDialog.FileName);
            }
            else
            {
                return;
            }

            // 2-Ako je slika locirana proveravam da li se nalazu u Slike direktorijumu, ako ne kopiram je tu
            if (folder!=GlobalniKod.SlikeDir)
            {
                int dodatak = 1;
                string novoIme = imeSlike;
                while (File.Exists(Path.Combine(GlobalniKod.SlikeDir,novoIme)))
                {
                    novoIme = imeSlike + string.Format("_{0}", ++dodatak);
                }

                File.Copy(openFileDialog.FileName, Path.Combine(GlobalniKod.SlikeDir, novoIme));
                imeSlike = novoIme;
            }

            // 3-Dodajem sliku svojstvu
            this.Slika = imeSlike;
        }

        // UkloniSlikuCommand
        private DelegateCommand _ukloniSlikuCommand;
        public DelegateCommand UkloniSlikuCommand =>
            _ukloniSlikuCommand ?? (_ukloniSlikuCommand = new DelegateCommand(ExecuteUkloniSlikuCommand));


        void ExecuteUkloniSlikuCommand()
        {
            this.Slika = string.Empty;
        }


        // SlikaCommand
        private DelegateCommand _slikaCommand;
        public DelegateCommand SlikaCommand =>
            _slikaCommand ?? (_slikaCommand = new DelegateCommand(ExecuteSlikaCommand));

        void ExecuteSlikaCommand()
        {
            Views.Slika s = new Views.Slika(this.Slika, this.Sifra + " " +this.Naziv);
            s.Show();
        }
        #endregion

        #region Ctor
        public ArtikliEditViewModel(IDbService dbService, IRegionManager regionManager)
        {
            _dbService = dbService;
            _regionManager = regionManager;
        }
        #endregion

        #region Methods
        private void OsveziVelicineZaIzbor()
        {
            IEnumerable<Velicina> tempVel = _dbService.GetAllVelicine();

            ObservableCollection<VelicinaZaIzbor> osvezeneVelicineZaIzbor = new ObservableCollection<VelicinaZaIzbor>();

            foreach (Velicina item in tempVel)
            {
                bool stariIzbor = false;
                if (VelicineZaIzbor != null)
                {
                    var vecUnet = VelicineZaIzbor.FirstOrDefault(v => v.ID == item.ID);
                    stariIzbor = vecUnet == null ? false : vecUnet.Izbor;
                }
                osvezeneVelicineZaIzbor.Add(new VelicinaZaIzbor() { ID = item.ID, Oznaka = item.Oznaka, Izbor=stariIzbor });
            }

            VelicineZaIzbor = osvezeneVelicineZaIzbor;
            ExecuteVelicinaCommand();
        }

        private bool UslovZaUpis()
        {
            string poruka = "";
            if (string.IsNullOrWhiteSpace(Sifra))
                poruka += "Knjigovodstvena šifra je obavezan podatak.";

            if (string.IsNullOrWhiteSpace(Naziv))
                poruka += "\nNaziv artikla je obavezan podatak.";

            if (string.IsNullOrWhiteSpace(Jm))
                poruka += "\nJedinica mere je obavezan podatak.";

            if (!string.IsNullOrEmpty(poruka))
            {
                MessageBox.Show(poruka, "Nije ispunjen uslov za upis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
            {
                return true;
            }

        }

        private void SnimiVelicine()
        {
            //Formiram niz obelezenih velicina
            var velicineZaUpis =
                from v in VelicineZaIzbor
                where v.Izbor
                select v.ID;

            _dbService.InsertOrUpdateVelicineArtikla(_loadedID, velicineZaUpis);
        }

        private void NazadIUkloniViewIzMemorije(AkcijaEnum izvrsenaAkcija)
        {
            NavigationParameters navPar = new NavigationParameters();
            navPar.Add("ArtikalID", izvrsenaAkcija == AkcijaEnum.Odustani ? 0 : _loadedID);
            navPar.Add("JeIzbrisan", izvrsenaAkcija == AkcijaEnum.Izbrisi);

            _keepAlive = false;
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.Artikli, navPar);
        }

        private void UcitajArtikal(long artikalID)
        {
            Artikal artikal = _dbService.GetArtikal(artikalID);
            if (artikal!=null)
            {
                _loadedID = artikalID;
                this.Sifra = artikal.Sifra;
                this.Naziv = artikal.Naziv;
                this.Jm = artikal.Jm;
                this.BarKod = artikal.BarKod;
                this.Slika = artikal.Slika;

                this.Naslov = "Editovanje artikla";
            }
        }

        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
          var artikalID = navigationContext.Parameters["ArtikalID"];
            if (artikalID!=null)
            {
                long id = (long)artikalID;
                UcitajArtikal(id);

                var tempVel = _dbService.GetVelicine(id);
                if (tempVel.Any())
                {
                    VelicineZaIzbor = new ObservableCollection<VelicinaZaIzbor>();
                     foreach (var vel in tempVel)
                    {
                        VelicineZaIzbor.Add(new VelicinaZaIzbor() { ID = vel.ID, Oznaka = vel.Oznaka, Izbor = true });
                    }
               }
                
           }
            else
            {
                this.Jm = "par";
            }

            IzbrisiCommand.RaiseCanExecuteChanged();

            OsveziVelicineZaIzbor();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
        #endregion //INavigationAware

        #region IRegionMemberLifetime
        public bool KeepAlive => _keepAlive;

        #endregion
    }

    public class VelicinaZaIzbor : Velicina
    {
        public bool Izbor { get; set; }
    }
}
