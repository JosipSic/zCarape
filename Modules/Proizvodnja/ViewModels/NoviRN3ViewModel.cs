using Jezgro.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using zCarape.Core;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class NoviRN3ViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        #region Fields

        private readonly IDbService _dbService;
        private readonly IRegionManager _regionManager;
        private long _radniNalogID = 0;

        #endregion //Fields

        #region Properties

        // Naslov
        private string _naslov= "Otvaranje radnog naloga";
        public string Naslov
        {
            get { return _naslov; }
            set { SetProperty(ref _naslov, value); }
        }

        // Naslov2
        private string _naslov2 = "3/3";
        public string Naslov2
        {
            get { return _naslov2; }
            set { SetProperty(ref _naslov2, value); }
        }

        // IsEdit
        private bool _isEdit = false;
        public bool IsEdit
        {
            get { return _isEdit; }
            set { SetProperty(ref _isEdit, value); }
        }

        // Artikal
        private Artikal _artikal;
        public Artikal Artikal
        {
            get { return _artikal; }
            set { SetProperty(ref _artikal, value); }
        }

        // Dezen
        private DezenArtikla _dezen;
        public DezenArtikla Dezen
        {
            get { return _dezen; }
            set { SetProperty(ref _dezen, value); }
        }

        // Velicine
        private ObservableCollection<Velicina> _velicine;
        public ObservableCollection<Velicina> Velicine
        {
            get { return _velicine; }
            set { SetProperty(ref _velicine, value); }
        }

        // SelectedVelicina
        private Velicina _selectedVelicina;
        public Velicina SelectedVelicina
        {
            get { return _selectedVelicina; }
            set { SetProperty(ref _selectedVelicina, value); }
        }

        // Datum
        private DateTime _datum = DateTime.Now.Date;
        public DateTime Datum
        {
            get { return _datum; }
            set { SetProperty(ref _datum, value); }
        }

        // Cilj
        private long _cilj;
        public long Cilj
        {
            get { return _cilj; }
            set { SetProperty(ref _cilj, value); }
        }

        // Hitno
        private bool _hitno;
        public bool Hitno
        {
            get { return _hitno; }
            set { SetProperty(ref _hitno, value); }
        }

        // Podsetnik
        private string _podsetnik;
        public string Podsetnik
        {
            get { return _podsetnik; }
            set { SetProperty(ref _podsetnik, value); }
        }

        // NazadText
        private string _nazadText = "Nazad na izbor dezena";
        public string NazadText
        {
            get { return _nazadText; }
            set { SetProperty(ref _nazadText, value); }
        }

        // Masine
        private ObservableCollection<MasinaZaIzbor> _masine = new ObservableCollection<MasinaZaIzbor>();

        public ObservableCollection<MasinaZaIzbor> Masine
        {
            get { return _masine; }
            set { SetProperty(ref _masine, value); }
        }

        // Statusi
        private List<Status> _statusi;
        public List<Status> Statusi
        {
            get { return _statusi; }
            set { SetProperty(ref _statusi, value); }
        }

        // SelectedStatus
        private Status _selectedStatus;
        public Status SelectedStatus
        {
            get { return _selectedStatus; }
            set { SetProperty(ref _selectedStatus, value); }
        }

        private bool _isIzmena = false;
        public bool IsIzmena
        {
            get { return _isIzmena; }
            set { SetProperty(ref _isIzmena, value); }
        }

        #endregion //Properties

        #region Cmd

        // SnimiCommand
        private DelegateCommand _snimiCommand;
        public DelegateCommand SnimiCommand =>
            _snimiCommand ?? (_snimiCommand = new DelegateCommand(ExecuteSnimiCommand));

        void ExecuteSnimiCommand()
        {
            if (!UslovZaUpis())
                return;

            long odgovor = _dbService.InsertOrUpdateRadniNalog(new RadniNalog()
            {
                ID = this._radniNalogID,
                ArtikalID = Artikal.ID,
                DezenArtiklaID = Dezen.ID,
                VelicinaID = SelectedVelicina.ID,
                Cilj = this.Cilj,
                Status = this.SelectedStatus.StatusRN,
                Podsetnik = this.Podsetnik,
                Hitno = this.Hitno
            });
            if (odgovor > 0)
            {
                _radniNalogID = odgovor;
                SnimiMasine();
                if (!IsEdit) { MessageBox.Show($"Radni nalog je snimlje u bazu pod brojem {odgovor}", "Novi RN", MessageBoxButton.OK, MessageBoxImage.Information); }
                _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.MasineURadu);
            }
        }

        // BackCommand
        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand =>
            _backCommand ?? (_backCommand = new DelegateCommand(ExecuteBackCommand));

        void ExecuteBackCommand()
        {
            if (IsEdit)
            {
                _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.MasineURadu);
            }
            else
            {
                NavigationParameters param = new NavigationParameters();
                param.Add("Artikal", Artikal);
                if (Dezen != null)
                {
                    param.Add("DezenID", Dezen.ID);
                }
                _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.NoviRN2Dezen, param);
            }
        }

        // EditDezenCommand
        private DelegateCommand _editDezenCommand;
        public DelegateCommand EditDezenCommand =>
            _editDezenCommand ?? (_editDezenCommand = new DelegateCommand(ExecuteEditDezenCommand));

        void ExecuteEditDezenCommand()
        {
            if (Artikal == null || Dezen == null)
            {
                return;
            }

            GlobalniKod.DezenParam.Blanko();
            GlobalniKod.DezenParam.ArtikalID = Artikal.ID;
            GlobalniKod.DezenParam.ArtikalSifra = Artikal.Sifra;
            GlobalniKod.DezenParam.ArtikalNaziv = Artikal.Naziv;
            GlobalniKod.DezenParam.DezenArtiklaID = Dezen.ID;

            DezeniEdit dezeniEdit = new DezeniEdit();
            GlobalniKod.DezenParam.Window = dezeniEdit;
            dezeniEdit.ShowDialog();

            if (!GlobalniKod.DezenParam.VracenBezPromene)
            {
                Dezen = _dbService.GetDezenArtikla(Dezen.ID);
            }
        }

        // DeleteCommand
        private DelegateCommand _deleteCommand;
        public DelegateCommand DeleteCommand =>
            _deleteCommand ?? (_deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        void ExecuteDeleteCommand()
        {
            if (_radniNalogID>0 && _dbService.DeleteRadniNalog(_radniNalogID))
            {
                _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.MasineURadu);
            }
        }

        #endregion //Cmd

        #region Ctor

        public NoviRN3ViewModel(IDbService dbService, IRegionManager regionManager)
        {
            _dbService = dbService;
            _regionManager = regionManager;
            FormirajListuStatusa();
        }


        #endregion //Ctor

        #region Methods

        private bool UcitajRadniNalog(long radniNalogID)
        {
            RadniNalog rn = _dbService.GetRadniNalog(radniNalogID);
            if (rn==null)
            {
                return false;
            }

            this.Artikal = _dbService.GetArtikal(rn.ArtikalID);
            this.Dezen = _dbService.GetDezenArtikla(rn.DezenArtiklaID);
            this.NazadText = "Odustani";
            this.Naslov = "Radni nalog br. " + radniNalogID.ToString();
            this.IsEdit = true;
            this.Naslov2 = "";
           
            FormirajSpisakVelicina();
            FormirajSpisakMasina();

            SelectedVelicina = Velicine.FirstOrDefault(v => v.ID == rn.VelicinaID);
            SelectedStatus = Statusi.FirstOrDefault(s => s.StatusRN == rn.Status);
            if (SelectedStatus==null)
            {
                SelectedStatus = Statusi.FirstOrDefault(s => s.StatusRN == StatusRadnogNaloga.Aktivan);
            }
            Cilj = rn.Cilj;
            Hitno = rn.Hitno;
            Podsetnik = rn.Podsetnik;
            Datum = rn.VremeUnosa;


            return true;
        }

        private void SnimiMasine()
        {
            //Formiram niz obelezenih masina
            var masineZaUpis =
                from m in Masine
                where m.Izbor
                select m.ID;

            _dbService.InsertOrUpdateAngazovaneMasine(_radniNalogID, masineZaUpis);
        }

        private bool UslovZaUpis()
        {
            string poruka = "";
            if (SelectedVelicina == null)
                poruka += "Veličina je obavezan podatak.";

            if (SelectedStatus == null)
                poruka += "Status radnog naloga je obavezan podatak.";

            if (Cilj <= 0)
                poruka += "\nCiljna kolicina za proizvodnju je obavezan podatak.";

            if (!Masine.Any(m => m.Izbor))
                poruka += "\nMora biti izabrana barem jedna masina na koj" +
                    "oj se radi.";

            if (SelectedStatus.StatusRN == StatusRadnogNaloga.Pauziran && Hitno)
                poruka += "\nNalog ne moze biti istovremono i Hitan i Pauziran. Isključite opciju Hitan ili promenite status.";

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


        private void FormirajSpisakMasina()
        {
            var masine = new ObservableCollection<Masina>(_dbService.GetAllMasine());

            Masine.Clear();
            foreach (Masina item in masine)
            {
                Masine.Add(
                         new MasinaZaIzbor() { ID=item.ID, Naziv=item.Naziv}
                          );
            }

            if (_radniNalogID>0)
            {
                var xm = _dbService.GetAngazovaneMasinePoRadnomNalogu(_radniNalogID);

                foreach (AngazovanaMasina am in xm)
                {
                    var m = Masine.FirstOrDefault(m => m.ID == am.MasinaID);
                    if (m!=null)
                    {
                        m.Izbor = true;
                    }
                }
            }
        }

        private void FormirajSpisakVelicina()
        {
            Velicine = new ObservableCollection<Velicina>(_dbService.GetVelicine(Artikal.ID));
        }

        private void FormirajListuStatusa()
        {
            Statusi = new List<Status>(){
                new Status() {Naziv = "Aktivan", StatusRN=StatusRadnogNaloga.Aktivan },
                new Status() {Naziv = "Pauziran", StatusRN=StatusRadnogNaloga.Pauziran},
                new Status() {Naziv = "Zaključen", StatusRN=StatusRadnogNaloga.Zatvoren} 
            };
        }

        #endregion //Methods

        #region INavigationAware

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var izmena = navigationContext.Parameters["RadniNalogID"];
            if (izmena !=null)
            {
                this._radniNalogID = (long)izmena;
                if (!UcitajRadniNalog(_radniNalogID))
                {
                    MessageBox.Show($"Radni nalog {_radniNalogID} ne postoji","", MessageBoxButton.OK, MessageBoxImage.Information);
                    _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.MasineURadu);
                }
                IsIzmena = true;
            }
            else
            { 
                var artPar = navigationContext.Parameters["Artikal"];
                var dezPar = navigationContext.Parameters["Dezen"];
                if (artPar != null && dezPar != null)
                {
                    Artikal = artPar as Artikal;
                    Dezen = dezPar as DezenArtikla;

                    FormirajSpisakVelicina();
                    FormirajSpisakMasina();
                }
                else
                {
                    MessageBox.Show("Nisu prosledjeni ocekivani parametri");
                }
                SelectedStatus = Statusi.FirstOrDefault(s => s.StatusRN == StatusRadnogNaloga.Aktivan);
            }

        }
        #endregion //INavigationAware

        #region IRegionMemberLifetime

        public bool KeepAlive => false;

        #endregion //IRegionMemberLifetime
    }

    public class Status
    {
        public string Naziv { get; set; }
        public byte StatusRN { get; set; }
    }

    public class MasinaZaIzbor
    {
        public long ID { get; set; }
        public string Naziv { get; set; }
        public bool Izbor { get; set; }
    }
}
