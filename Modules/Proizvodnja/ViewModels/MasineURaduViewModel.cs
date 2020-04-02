using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using zCarape.Core.Models;
using zCarape.Core.Business;
using zCarape.Services.Interfaces;
using Prism.Regions;
using zCarape.Core;
using Prism.Events;

namespace Proizvodnja.ViewModels
{
    public class MasineURaduViewModel : BindableBase, IRegionMemberLifetime
    {
        #region Fields

        private readonly IDbService _dbService;
        private readonly IRegionManager _regionManager;

        #endregion // Fields

        #region Properties

        // MasineURadu
        private ObservableCollection<MasinaURadu> _masineURadu;
        public ObservableCollection<MasinaURadu> MasineURadu
        {
            get { return _masineURadu; }
            set { SetProperty(ref _masineURadu, value); }
        }

        // Radnici
        private ObservableCollection<Lice> _radnici;
        public ObservableCollection<Lice> Radnici
        {
            get { return _radnici; }
            set { SetProperty(ref _radnici, value); }
        }

        #endregion //Properties

        #region Commands

        // SlikaCommand
        private DelegateCommand<object> _slikaCommand;
        public DelegateCommand<object> SlikaCommand =>
            _slikaCommand ?? (_slikaCommand = new DelegateCommand<object>(ExecuteSlikaCommand));


        void ExecuteSlikaCommand(object param)
        {
            if (param!=null)
            {
                var values = (object[])param;
                var slika = (string)values[0];
                var nalog = (NalogURadu)values[1];

                Jezgro.Views.Slika s = new Jezgro.Views.Slika(slika, nalog.ArtikalSifra + " "+ nalog.ArtikalNaziv, nalog.ArtikalDezen);
                s.Show();
            }
        }

        // EditNalogCommand
        private DelegateCommand<long?> _editNalogCommand;
        public DelegateCommand<long?> EditNalogCommand =>
            _editNalogCommand ?? (_editNalogCommand = new DelegateCommand<long?>(ExecuteEditNalogCommand));

        void ExecuteEditNalogCommand(long? parameter)
        {
            if (parameter==null || parameter==0)
            {
                return;
            }
            NavigationParameters param = new NavigationParameters();
            param.Add("RadniNalogID", parameter);
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.NoviRN3,param);
        }

        #endregion //Commands

        #region Ctor

        public MasineURaduViewModel(IDbService dbService, IRegionManager regionManager)
        {
            _dbService = dbService;
            _regionManager = regionManager;
            FormirajListuRadnika();
            FormirajSpisakMasinaURadu();
        }


        #endregion //Ctor

        #region Methods

        private void FormirajSpisakMasinaURadu()
        {
            MasineURadu = new ObservableCollection<MasinaURadu>(_dbService.GetAllMasineURadu());
        }

        private void FormirajListuRadnika()
        {
            Radnici = new ObservableCollection<Lice>(_dbService.GetAllAktivnaLica());
        }

        private void AzurirajPrijemnicu(Zadatak zadatak)
        {
            
        }


        #endregion //Methods

        #region IRegionMemberLifetime
        public bool KeepAlive => false;

        #endregion //IRegionMemberLifetime

    }
}
