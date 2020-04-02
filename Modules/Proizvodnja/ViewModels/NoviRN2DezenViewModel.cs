using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using zCarape.Core;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class NoviRN2DezenViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        #region Fields

        private readonly IRegionManager _regionManager;
        private readonly IDbService _dbService;
        private ICollectionView _dezeniCollectionView;

        #endregion //Fields

        #region Propereties

        // NazivDezena
        private string _nazivDezena = "Izaberite dezen";
        public string NazivDezena
        {
            get { return _nazivDezena; }
            set { SetProperty(ref _nazivDezena, value); }
        }

        // Artikal
        private Artikal _artikal;
        public Artikal Artikal
        {
            get { return _artikal; }
            set { SetProperty(ref _artikal, value); }
        }

        // Dezeni
        private ObservableCollection<DezenArtikla> _dezeni;
        public ObservableCollection<DezenArtikla> Dezeni
        {
            get { return _dezeni; }
            set { SetProperty(ref _dezeni, value); }
        }

        // SelectedDezen
        private DezenArtikla _selectedDezen;
        public DezenArtikla SelectedDezen
        {
            get { return _selectedDezen; }
            set { SetProperty(ref _selectedDezen, value);
                if (_selectedDezen == null)
                {
                    NazivDezena = "Izaberite dezen";
                }
                else
                {
                    NazivDezena = string.Format($"Izabrani dezen: {SelectedDezen.Naziv}");
                }
            }
        }

        // FilterDerzeniString
        private string _filterDezeniString;
        public string FilterDezeniString
        {
            get { return _filterDezeniString; }
            set
            {
                SetProperty(ref _filterDezeniString, value);
                if (_dezeniCollectionView != null)
                {
                    _dezeniCollectionView.Refresh();
                }
            }
        }

        #endregion //Properties

        #region Cmd

        // NextCommand
        private DelegateCommand _nextCommand;
        public DelegateCommand NextCommand =>
            _nextCommand ?? (_nextCommand = new DelegateCommand(ExecuteNextCommand, CanExecuteNextCommand)).ObservesProperty(() => SelectedDezen);

        void ExecuteNextCommand()
        {
            if (SelectedDezen == null || SelectedDezen.ID == 0)
            {
                return;
            }
            NavigationParameters param = new NavigationParameters();
            param.Add("Artikal", Artikal);
            param.Add("Dezen", SelectedDezen);
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.NoviRN3, param);
        }

        bool CanExecuteNextCommand()
        {
            return SelectedDezen != null;
        }

        // BackCommand
        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand =>
            _backCommand ?? (_backCommand = new DelegateCommand(ExecuteBackCommand));

        void ExecuteBackCommand()
        {
            NavigationParameters param = new NavigationParameters();
            param.Add("ArtikalID", Artikal.ID);
            if (SelectedDezen!=null)
            {
                param.Add("DezenID", SelectedDezen.ID);
            }
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.NoviRN1Artikal, param);
        }

        #endregion //Cmd

        #region Ctor
        public NoviRN2DezenViewModel(IRegionManager regionManager, IDbService dbService)
        {
            _regionManager = regionManager;
            _dbService = dbService;
        }

        #endregion //Ctor

        #region Methods

        private void FormirajSpisakDezena()
        {
            Dezeni = new ObservableCollection<DezenArtikla>(_dbService.GetDezeniArtikla(Artikal.ID));
            _dezeniCollectionView = CollectionViewSource.GetDefaultView(Dezeni);
            _dezeniCollectionView.Filter = new Predicate<object>(FilterDezeni);
        }

        private bool FilterDezeni(object obj)
        {
            var data = obj as DezenArtikla;
            if (data != null)
            {
                if (!string.IsNullOrWhiteSpace(FilterDezeniString))
                {
                    return data.Naziv.Contains(FilterDezeniString, StringComparison.InvariantCultureIgnoreCase);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        #endregion //Method

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
            var artPar = navigationContext.Parameters["Artikal"];
            if (artPar != null)
            {
                Artikal = artPar as Artikal;
                FormirajSpisakDezena();

                // Ako je prosledjen DezenID onda proveravam da li taj Dezen postoji u spsku za artikal
                var dezPar = navigationContext.Parameters["DezenID"];
                if (dezPar != null)
                {
                    var dezenID = (long)dezPar;
                    var izabraniDezen = Dezeni.FirstOrDefault(d => d.ID == dezenID);
                    if (izabraniDezen!=null && izabraniDezen.ArtikalID==Artikal.ID)
                    {
                        SelectedDezen = izabraniDezen;
                    }
                }
            }
            else
            {
                MessageBox.Show("Nije prosledjen ocekivani parametar");
            }

        }

        #endregion //INavigationAware

        #region IRegionMemberLifetime

        public bool KeepAlive => false;

        #endregion //IRegionMemberLifetime
    }
}
