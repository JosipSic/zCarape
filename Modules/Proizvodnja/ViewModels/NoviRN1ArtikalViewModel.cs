using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using zCarape.Core;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class NoviRN1ArtikalViewModel : BindableBase, IRegionMemberLifetime, INavigationAware
    {
        #region Fields

        private readonly IDbService _dbService;
        private readonly IRegionManager _regionManager;
        private ICollectionView _artikliCollectionView;
        private long _prosledjenDezenID = 0;

        #endregion //Fields

        #region Properties

        //Podnaslov
        private string _podnaslov = "Izaberite artikal";
        public string Podnaslov
        {
            get { return _podnaslov; }
            set { SetProperty(ref _podnaslov, value); }
        }

        //Artikli
        private ObservableCollection<Artikal> _artikli;

        public ObservableCollection<Artikal> Artikli
        {
            get { return _artikli; }
            set { SetProperty(ref _artikli, value); }
        }

        //SelectedArtikal
        private Artikal _selectedArtikal ;
        public Artikal SelectedArtikal
        {
            get { return _selectedArtikal; }
            set { SetProperty(ref _selectedArtikal, value);
                if (_selectedArtikal==null)
                {
                    Podnaslov = "Izaberite artikal";
                }
                else
                {
                    Podnaslov = string.Format($"Artikal za proizvodnju: {SelectedArtikal.Sifra} {_selectedArtikal.Naziv}");
                }
            }
        }

        // FilterArtikliString
        private string _filterArtikliString;
        public string FilterArtikliString
        {
            get { return _filterArtikliString; }
            set
            {
                SetProperty(ref _filterArtikliString, value);
                if (_artikliCollectionView != null)
                {
                    _artikliCollectionView.Refresh();
                }
            }
        }


        #endregion //Properties

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
                    return data.Naziv.Contains(FilterArtikliString, StringComparison.InvariantCultureIgnoreCase)
                        || data.Sifra.Contains(FilterArtikliString, StringComparison.InvariantCulture);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        #endregion //Methods

        #region Cmd

        private DelegateCommand _nextCommand;
        public DelegateCommand NextCommand =>
            _nextCommand ?? (_nextCommand = new DelegateCommand(ExecuteNextCommand, CanExecuteNextCommand)).ObservesProperty(()=>SelectedArtikal);

        void ExecuteNextCommand()
        {
            if (SelectedArtikal==null || SelectedArtikal.ID==0)
            {
                return;
            }
            NavigationParameters param = new NavigationParameters();
            param.Add("Artikal", SelectedArtikal);

            if (_prosledjenDezenID>0)
            {
                param.Add("DezenID", _prosledjenDezenID);
            }

           _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.NoviRN2Dezen, param);
        }

        bool CanExecuteNextCommand()
        {
            return SelectedArtikal!=null;
        }

        #endregion //Cmd

        #region Ctor
        public NoviRN1ArtikalViewModel(IDbService dbService, IRegionManager regionManager)
        {
            _dbService = dbService;
            _regionManager = regionManager;
            FormirajSpisakArtikala();
        }
        #endregion //Ctor

        #region IRegionMemberLifetime
        public bool KeepAlive => false;
        #endregion //IRegionMemberLifetime

        #region INavigationAware

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var artPar = navigationContext.Parameters["ArtikalID"];
            if (artPar != null)
            {
                var artikalID = (long)artPar;
                var izabraniArt = Artikli.FirstOrDefault(a => a.ID == artikalID);
                if (izabraniArt != null) { SelectedArtikal = izabraniArt; }
            }

            // Ako je forma pozvana iz Izbora dezena pamtim dezen koji je bio izabran
            var dezPar = navigationContext.Parameters["DezenID"];
            if (dezPar != null)
            {
                _prosledjenDezenID = (long)dezPar;
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
