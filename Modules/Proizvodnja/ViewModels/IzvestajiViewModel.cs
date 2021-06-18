using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using zCarape.Core.Business;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class IzvestajiViewModel : BindableBase, IRegionMemberLifetime
    {
        enum RasponEnum
        {
            SvaDeca=0,
            ZaPeriod=1
        }

        #region Fields
        private readonly IDbService _dbService;
        private bool _programskaPromenaDatuma = false;
        #endregion

        #region Properies
        // Stavke
        private IEnumerable<IzvestajStavka> _stavke;
        public IEnumerable<IzvestajStavka> Stavke
        {
            get { return _stavke; }
            set { SetProperty(ref _stavke, value); }
        }

        // DatumOd
        private DateTime? _datumOd;
        public DateTime? DatumOd
        {
            get { return _datumOd; }
            set { SetProperty(ref _datumOd, value); KreirajListu(); }
        }

        // DatumDo
        private DateTime? _datumDo;
        public DateTime? DatumDo
        {
            get { return _datumDo; }
            set { SetProperty(ref _datumDo, value); KreirajListu(); }
        }

        // Period
        private int _period = (int)RasponEnum.ZaPeriod;
        public int Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value); KreirajListu(); }
        }

        public bool KeepAlive => false;

        #endregion

        #region Ctor
        public IzvestajiViewModel(IDbService dbService)
        {
            _dbService = dbService;
        }
        #endregion

        #region Commands
        private DelegateCommand<string> _navigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));


        void ExecuteNavigateCommand(string smer)
        {
            if (smer=="Napred")
            {
                PomeriPeriod(1);
            }
            else if (smer=="Nazad")
            {
                PomeriPeriod(-1);
            }
        }

        #endregion

        #region Methods
        private void KreirajListu()
        {
            if (_programskaPromenaDatuma)
                return;

            DateTime? parOd=null;
            DateTime? parDo=null;

            if (Period == (int)RasponEnum.ZaPeriod)
            {
                if (DatumOd==null || DatumDo==null)
                {
                    Stavke = null;
                    return;
                }

                parOd = DatumOd;
                parDo = DatumDo;
            }

            Stavke = _dbService.GetStavkeZaIzvestaj(parOd,parDo);
        }

        private void PomeriPeriod(int move = 1)
        {
            _programskaPromenaDatuma = true;
            if (DatumOd == null && DatumDo == null)
            {
                DateTime postaviMesec = (move < 0) ? DateTime.Now.Date.AddMonths(move) : DateTime.Now;
                DatumOd = new DateTime(postaviMesec.Year, postaviMesec.Month, 1);
                DatumDo = new DateTime(postaviMesec.Year, postaviMesec.Month, DateTime.DaysInMonth(postaviMesec.Year, postaviMesec.Month));
            }
            else if (DatumOd == null)
            {
                DatumOd = new DateTime(DatumDo.Value.Year, DatumDo.Value.Month, 1);
            }
            else if (DatumDo == null)
            {
                DatumDo = new DateTime(DatumOd.Value.Year, DatumOd.Value.Month, DateTime.DaysInMonth(DatumOd.Value.Year, DatumOd.Value.Month));
            }
            else
            {

                if (DatumOd.Value.Day == 1 && DatumDo.Value.Day == DateTime.DaysInMonth(DatumDo.Value.Year,DatumDo.Value.Month))
                {
                    int m = (DatumDo.Value.Year * 12 + DatumDo.Value.Month - DatumOd.Value.Year * 12 - DatumOd.Value.Month)*move;
                    move += m;

                    DatumOd = DatumOd.Value.AddMonths(move);
                    DateTime prviDanDrugogDatuma = new DateTime(DatumDo.Value.Year, DatumDo.Value.Month, 1);
                    DatumDo = prviDanDrugogDatuma.AddMonths(move+1).AddDays(-1);
                }
                else
                {
                    int razlika = ((DatumDo.Value - DatumOd.Value).Days + 1) * move ;
                    DatumOd = DatumOd.Value.AddDays(razlika);
                    DatumDo = DatumDo.Value.AddDays(razlika);
                }
            }
            _programskaPromenaDatuma = false;
            KreirajListu();
        }

        #endregion
    }
}
