using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using zCarape.Core.Business;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class PivotViewModel : BindableBase
    {
        #region Properties
        // Stavke
        private IEnumerable<IzvestajStavka> _stavke;
        private readonly IDbService _dbService;

        public IEnumerable<IzvestajStavka> Stavke
        {
            get { return _stavke; }
            set { SetProperty(ref _stavke, value); }
        }
        #endregion

        #region Ctor
        public PivotViewModel(IDbService dbService)
        {
            _dbService = dbService;
            KreirajListu();
        }
        #endregion

        #region Methods
        private void KreirajListu()
        {

            Stavke = _dbService.GetStavkeZaIzvestaj();
        }
        #endregion
    }
}
