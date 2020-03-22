using Jezgro.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using zCarape.Core;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Jezgro.ViewModels
{
    public class ArtikliViewModel : BindableBase, INavigationAware
    {
        #region Fields
        private readonly IDbService _dbService;
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;
        private ICollectionView _artikliCollectionView;

        #endregion //Fields ---------------------------------------------------------

        #region Properties

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
                    PopuniVelicineSelektovanogArtikla();
                    PopuniDezeneSelektovanogArtikla();
                   
                    if (string.IsNullOrWhiteSpace(value.Slika))
                        PunaPutanjaDoSlike = string.Empty;
                    else
                        PunaPutanjaDoSlike = Path.Combine(GlobalniKod.SlikeDir, value.Slika);
                    IsSelectedArtikal = true;
                }
                else
                {
                    IsSelectedArtikal = false;
                }
            }
        }

        // IsSelectedArtikal
        private bool _isSelectedArtikal;
        public bool IsSelectedArtikal
        {
            get { return _isSelectedArtikal; }
            set { 
                SetProperty(ref _isSelectedArtikal, value);
                IsNotSelectedArtikal = !value; 
            }
        }

        // IsSelectedArtikal
        private bool _isNotSelectedArtikal = true;
        public bool IsNotSelectedArtikal
        {
            get { return _isNotSelectedArtikal; }
            set { SetProperty(ref _isNotSelectedArtikal, value); }
        }


        // FilterArtikliString
        private string _filterArtikliString;
        public string FilterArtikliString
        {
            get { return _filterArtikliString; }
            set { SetProperty(ref _filterArtikliString, value);
                if (_artikliCollectionView!=null)
                {
                    _artikliCollectionView.Refresh();
                }
            }
        }

        // PunaPutanjaDoSlike
        private string _punaPutanjaDoSlike;
        public string PunaPutanjaDoSlike
        {
            get { return _punaPutanjaDoSlike; }
            set { SetProperty(ref _punaPutanjaDoSlike, value); }
        }

        // TextIsNotSelected
        private string _textIsNotSelected = "Izaberite artikal za prikaz ili unesite novi";
        public string TextIsNotSelected
        {
            get { return _textIsNotSelected; }
            set { SetProperty(ref _textIsNotSelected, value); }
        }


        #endregion //Properties ------------------------------------------------------

        #region Cmd
        // NoviArtikalCommand
        private DelegateCommand _noviArtikalCommand;
        public DelegateCommand NoviArtikalCommand =>
            _noviArtikalCommand ?? (_noviArtikalCommand = new DelegateCommand(ExecuteNoviArtikalCommand));

        void ExecuteNoviArtikalCommand()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.ArtikliEdit);
        }

        // EditArtikalCommand
        private DelegateCommand _editArtikalCommand;
        public DelegateCommand EditArtikalCommand =>
            _editArtikalCommand ?? (_editArtikalCommand = new DelegateCommand(ExecuteEditArtikalCommand));

        void ExecuteEditArtikalCommand()
        {
            if (SelectedArtikal == null)
                return;

            NavigationParameters navPar = new NavigationParameters();
            navPar.Add("ArtikalID", SelectedArtikal.ID);
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.ArtikliEdit, navPar);
        }

        // EditDezenCommand
        private DelegateCommand<long?> _editDezenCommand;
        public DelegateCommand<long?> EditDezenCommand =>
            _editDezenCommand ?? (_editDezenCommand = new DelegateCommand<long?>(ExecuteEditDezenCommand));

        void ExecuteEditDezenCommand(long? param)
        {
            if (SelectedArtikal==null)
            {
                return;
            }

            GlobalniKod.DezenParam.Blanko();
            GlobalniKod.DezenParam.ArtikalID = SelectedArtikal.ID;
            GlobalniKod.DezenParam.ArtikalSifra = SelectedArtikal.Sifra;
            GlobalniKod.DezenParam.ArtikalNaziv = SelectedArtikal.Naziv;
            GlobalniKod.DezenParam.DezenArtiklaID = param==null ? 0 : (long)param;

            DezeniEdit dezeniEdit = new DezeniEdit();
            GlobalniKod.DezenParam.Window = dezeniEdit;
            dezeniEdit.ShowDialog();

            if (!GlobalniKod.DezenParam.VracenBezPromene)
            {
                PopuniDezeneSelektovanogArtikla();
            }
        }

        // SlikaCommand
        private DelegateCommand<string> _slikaCommand;
        public DelegateCommand<string> SlikaCommand =>
            _slikaCommand ?? (_slikaCommand = new DelegateCommand<string>(ExecuteSlikaCommand));

        void ExecuteSlikaCommand(string image)
        {
            if (image==null)
            {
                if (!string.IsNullOrWhiteSpace(this.SelectedArtikal.Slika))
                {
                    Views.Slika s = new Views.Slika(this.SelectedArtikal.Slika, this.SelectedArtikal.Sifra + " " + this.SelectedArtikal.Naziv);
                    s.Show();
                }
            }
            else if (!string.IsNullOrWhiteSpace(image))
            {
                Views.Slika s = new Views.Slika(image, this.SelectedArtikal.Sifra + " " + this.SelectedArtikal.Naziv);
                s.Show();
            }
        }
        #endregion

        #region Methods

        private void FormirajSpisakArtikala()
        {
            Artikli = new ObservableCollection<Artikal>(_dbService.GetAllArtikli());
            _artikliCollectionView = CollectionViewSource.GetDefaultView(Artikli);
            _artikliCollectionView.Filter = new Predicate<object>(FilterArtikli);
        }

        private bool FilterArtikli(object obj)
        {
            var data = obj as Artikal;
            if (data != null)
            {
                if (!string.IsNullOrWhiteSpace(FilterArtikliString))
                {
                    return data.Naziv.Contains(FilterArtikliString,StringComparison.InvariantCultureIgnoreCase) 
                        || data.Sifra.Contains(FilterArtikliString,StringComparison.InvariantCulture);     
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private void PopuniVelicineSelektovanogArtikla()
        {
            if (SelectedArtikal==null)
            {
                this.Velicine = new ObservableCollection<Velicina>();
            }
            else
            {
                this.Velicine = new ObservableCollection<Velicina>(_dbService.GetVelicine(SelectedArtikal.ID));
            }
        }

        private void PopuniDezeneSelektovanogArtikla()
        {
            if (SelectedArtikal==null)
            {
                this.Dezeni = new ObservableCollection<DezenArtikla>();
            }
            else
            {
                this.Dezeni = new ObservableCollection<DezenArtikla>(_dbService.GetDezeniArtikla(SelectedArtikal.ID));
            }
        }

        private void Blanko()
        {
            this.SelectedArtikal = new Artikal();
        }

        #endregion //Methods

        #region Ctor

        public ArtikliViewModel(IDbService dbService, IRegionManager regionManager, IDialogService dialogService)
        {
            _dbService = dbService;
            _regionManager = regionManager;
            _dialogService = dialogService;
        }

        #endregion //Ctor

        #region INavigationAware

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var artikalID = navigationContext.Parameters["ArtikalID"];

            if (artikalID==null)
            {
                if (SelectedArtikal==null)
                {
                    FormirajSpisakArtikala();
                }
                return;
            }

            var jeIzbrisan = navigationContext.Parameters["JeIzbrisan"];
            bool deleted = false;
            if (jeIzbrisan!=null)
            {
                deleted = (bool)jeIzbrisan;
            }

            long id = (long)artikalID;

            // Ako je parametar ArtikalID=0 onda nista nije menjano
            if (id!=0)
            {
                if (deleted)
                {
                    TextIsNotSelected = String.Format("Artikal {0} {1} je izbrisan!", SelectedArtikal.Sifra, SelectedArtikal.Naziv);
                }

                FormirajSpisakArtikala();

                if (deleted)
                {
                    SelectedArtikal = null;
                }
                else
                {
                    Artikal editovanArtikal = Artikli.FirstOrDefault(a => a.ID == id);
                    if (editovanArtikal.ID!=0)
                    {
                        SelectedArtikal = editovanArtikal;
                    }
                }
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        #endregion //INavigationAware
    }
}
