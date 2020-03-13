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

namespace Jezgro.ViewModels
{
    public class VelicineViewModel : BindableBase
    {
        private readonly IDbService _dBservice;
        private readonly IRegionManager _regionManager;
        private string _oznaka;
        public string Oznaka
        {
            get { return _oznaka; }
            set { 
                SetProperty(ref _oznaka, value);
                SnimiCommand.RaiseCanExecuteChanged(); 
            }
        }

        private ObservableCollection<Velicina> _velicine;
        public ObservableCollection<Velicina> Velicine
        {
            get { return _velicine; }
            set { SetProperty(ref _velicine, value); }
        }

        private Velicina _selectedVelicina;
        public Velicina SelectedVelicina
        {
            get { return _selectedVelicina; }
            set { SetProperty(ref _selectedVelicina, value); }
        }

        private DelegateCommand _snimiCommand;
        public DelegateCommand SnimiCommand =>
            _snimiCommand ?? (_snimiCommand = new DelegateCommand(ExecuteSnimiCommand, CanExecuteSnimiCommand));

        void ExecuteSnimiCommand()
        {
            long odgovor;
            odgovor = _dBservice.InsertOrUpdateVelicina(new Velicina() { Oznaka = this.Oznaka });

            if (odgovor>0)
            {
                Oznaka = string.Empty;
                FormirajSpisakVelicina();
            }
        }

        bool CanExecuteSnimiCommand()
        {
            return !string.IsNullOrWhiteSpace(_oznaka);
        }

        private DelegateCommand _izbrisiCommand;
        public DelegateCommand IzbrisiCommand =>
            _izbrisiCommand ?? (_izbrisiCommand = new DelegateCommand(ExecuteIzbrisiCommand));

        void ExecuteIzbrisiCommand()
        {
            if (_dBservice.IzbrisiVelicinu(SelectedVelicina.ID))
                FormirajSpisakVelicina();
        }

        private DelegateCommand _nazadCommand;
        public DelegateCommand NazadCommand =>
            _nazadCommand ?? (_nazadCommand = new DelegateCommand(ExecuteNazadCommand));

        void ExecuteNazadCommand()
        {
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add("View", "Velicine");
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.ArtikliEdit, navigationParameters);
        }

        public VelicineViewModel(IDbService iDBservice, IRegionManager regionManager)
        {
            _dBservice = iDBservice;
            _regionManager = regionManager;
            FormirajSpisakVelicina();
        }

        private void FormirajSpisakVelicina()
        {
            Velicine = new ObservableCollection<Velicina>(_dBservice.GetAllVelicine());
        }
    }
}
