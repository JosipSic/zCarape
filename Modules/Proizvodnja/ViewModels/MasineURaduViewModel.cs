using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class MasineURaduViewModel : BindableBase
    {
        #region Fields

        private readonly IDbService _dbService;

        #endregion // Fields

        #region Properties

        private ObservableCollection<MasinaURadu> _masineURadu;
        public ObservableCollection<MasinaURadu> MasineURadu
        {
            get { return _masineURadu; }
            set { SetProperty(ref _masineURadu, value); }
        }

        #endregion //Properties

        #region Ctor

        public MasineURaduViewModel(IDbService dbService)
        {
            _dbService = dbService;

            // Formiraj spisak svih masina
            FormirajSpisakMasinaURadu();
        }

        #endregion //Ctor

        #region Methods

        private void FormirajSpisakMasinaURadu()
        {
            MasineURadu = new ObservableCollection<MasinaURadu>(_dbService.GetAllMasineURadu());
        }

        #endregion //Methods

    }
}
