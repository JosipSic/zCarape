using Jezgro.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using zCarape.Core;
using zCarape.Core.Business;
using zCarape.Core.Models;
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
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;
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

        // SelectedStavka
        private IzvestajStavka _selectedStavka;
        public IzvestajStavka SelectedStavka
        {
            get { return _selectedStavka; }
            set { SetProperty(ref _selectedStavka, value); }
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

        public bool KeepAlive { get; set; } = false;

        #endregion

        #region Ctor
        public IzvestajiViewModel(IDbService dbService, IRegionManager regionManager, IDialogService dialogService)
        {
            _dbService = dbService;
            _regionManager = regionManager;
            _dialogService = dialogService;
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

        // EditNalogCommand
        private DelegateCommand _editNalogCommand;
        public DelegateCommand EditNalogCommand =>
            _editNalogCommand ?? (_editNalogCommand = new DelegateCommand(ExecuteEditNalogCommand));

        void ExecuteEditNalogCommand()
        {
            if (SelectedStavka==null)
            {
                return;
            }
            NavigationParameters param = new NavigationParameters();
            param.Add("RadniNalogID", SelectedStavka.RadniNalog);
            param.Add("GoToView", ViewNames.Izvestaji);
            KeepAlive = true;
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.NoviRN3, param);
            KeepAlive = false;
        }

        // PregledRnCommand
        private DelegateCommand _pregledRnCommand;
        public DelegateCommand PregledRnCommand =>
            _pregledRnCommand ?? (_pregledRnCommand = new DelegateCommand(ExecutePregledRnCommand));

        void ExecutePregledRnCommand()
        {
            if (SelectedStavka==null)
            {
                return;
            }
            IDialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("ID", SelectedStavka.RadniNalog);
            _dialogService.Show("PregledRnDijalog", dialogParameters, null);
        }

        // EditDezenCommand
        private DelegateCommand _editDezenCommand;
        public DelegateCommand EditDezenCommand =>
            _editDezenCommand ?? (_editDezenCommand = new DelegateCommand(ExecuteEditDezenCommand));

        void ExecuteEditDezenCommand()
        {
            if (SelectedStavka == null)
            {
                return;
            }

            RadniNalog rn = _dbService.GetRadniNalog(SelectedStavka.RadniNalog);
            if (rn==null || rn.DezenArtiklaID==0)
            {
                return;
            }

            GlobalniKod.DezenParam.Blanko();
            GlobalniKod.DezenParam.ArtikalID = rn.ArtikalID;
            GlobalniKod.DezenParam.ArtikalSifra = SelectedStavka.Sifra;
            GlobalniKod.DezenParam.ArtikalNaziv = SelectedStavka.Naziv;
            GlobalniKod.DezenParam.DezenArtiklaID = rn.DezenArtiklaID;

            DezeniEdit dezeniEdit = new DezeniEdit();
            GlobalniKod.DezenParam.Window = dezeniEdit;
            dezeniEdit.ShowDialog();

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
