using DevExpress.Xpf.PivotGrid;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using zCarape.Core.Business;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class PivotViewModel : BindableBase, IRegionMemberLifetime
    {
        enum RasponEnum
        {
            SvaDeca = 0,
            ZaPeriod = 1
        }
        private bool _programskaPromenaDatuma = false;

        #region Properties
        // DataSource
        private IEnumerable<IzvestajStavka> _dataSource;
        private readonly IDbService _dbService;

        public IEnumerable<IzvestajStavka> DataSource
        {
            get { return _dataSource; }
            set { SetProperty(ref _dataSource, value); }
        }

        // The collection of Pivot Grid fields.
        public ObservableCollection<Field> Fields { get; private set; }

        // The collection of Pivot Grid groups.
        public ObservableCollection<FieldGroup> Groups { get; private set; }

        // DatumOd
        private DateTime? _datumOd;
        public DateTime? DatumOd
        {
            get { return _datumOd; }
            set { SetProperty(ref _datumOd, value); UcitajPodatke(); }
        }

        // DatumDo
        private DateTime? _datumDo;
        public DateTime? DatumDo
        {
            get { return _datumDo; }
            set { SetProperty(ref _datumDo, value); UcitajPodatke(); }
        }

        // Period
        private int _period = (int)RasponEnum.ZaPeriod;
        public int Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value); UcitajPodatke(); }
        }

        public bool KeepAlive => false;
        #endregion

        #region Ctor
        public PivotViewModel(IDbService dbService)
        {
            _dbService = dbService;
            PostaviPoljaZaPivot();
            DatumOd = new DateTime(DateTime.Now.Year, 1, 1);
            DatumDo = new DateTime(DateTime.Now.Year, 12, 31);
        }
        #endregion

        #region Commands
        private DelegateCommand<string> _navigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));


        void ExecuteNavigateCommand(string smer)
        {
            if (smer == "Napred")
            {
                PomeriPeriod(1);
            }
            else if (smer == "Nazad")
            {
                PomeriPeriod(-1);
            }
        }

        #endregion


        #region Methods
        private void PostaviPoljaZaPivot()
        {
            Fields = new ObservableCollection<Field>() {
                new Field() { FieldName="Naziv", AreaIndex=0, FieldArea = FieldArea.RowArea, Name="fieldNaziv", FieldCaption="Artikal"},
                new Field() { FieldName="Datum", AreaIndex=1, FieldArea = FieldArea.ColumnArea, Name="fieldDatum",
                    Interval = FieldGroupInterval.DateMonthYear},
                new Field() { FieldName="Kolicina", AreaIndex=0, FieldArea = FieldArea.DataArea, Name="fieldKolicina" },

                new Field() { FieldName="Dezen", AreaIndex=0, FieldArea= FieldArea.FilterArea, Name="fieldDezen"},
                new Field() { FieldName="Velicina", AreaIndex=1, FieldArea= FieldArea.FilterArea, Name="fieldVelicina"},
                new Field() { FieldName="Masina", AreaIndex=2, FieldArea= FieldArea.FilterArea, Name="fieldMasina"},
                new Field() { FieldName="Smena", AreaIndex=3, FieldArea= FieldArea.FilterArea, Name="fieldSmena"},
                new Field() { FieldName="Radnik", AreaIndex=4, FieldArea= FieldArea.FilterArea, Name="fieldRadnik"},
                new Field() { FieldName="DrugaKl", AreaIndex=5, FieldArea= FieldArea.FilterArea, Name="fieldDrugaKl"}
          };

            Groups = new ObservableCollection<FieldGroup>() {
                new FieldGroup() { GroupName = "groupYearMonth" }
           };
        }

        private void UcitajPodatke()
        {
            if (_programskaPromenaDatuma)
                return;

            DateTime? parOd = null;
            DateTime? parDo = null;

            if (Period == (int)RasponEnum.ZaPeriod)
            {
                if (DatumOd == null || DatumDo == null)
                {
                    DataSource = null;
                    return;
                }

                parOd = DatumOd;
                parDo = DatumDo;
            }

            DataSource = _dbService.GetStavkeZaIzvestaj(parOd, parDo);
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

                if (DatumOd.Value.Day == 1 && DatumDo.Value.Day == DateTime.DaysInMonth(DatumDo.Value.Year, DatumDo.Value.Month))
                {
                    int m = (DatumDo.Value.Year * 12 + DatumDo.Value.Month - DatumOd.Value.Year * 12 - DatumOd.Value.Month) * move;
                    move += m;

                    DatumOd = DatumOd.Value.AddMonths(move);
                    DateTime prviDanDrugogDatuma = new DateTime(DatumDo.Value.Year, DatumDo.Value.Month, 1);
                    DatumDo = prviDanDrugogDatuma.AddMonths(move + 1).AddDays(-1);
                }
                else
                {
                    int razlika = ((DatumDo.Value - DatumOd.Value).Days + 1) * move;
                    DatumOd = DatumOd.Value.AddDays(razlika);
                    DatumDo = DatumDo.Value.AddDays(razlika);
                }
            }
            _programskaPromenaDatuma = false;
            UcitajPodatke();
        }

        #endregion
    }

    public class Field
    {
        public string FieldName { get; set; }
        public string Name { get; set; }
        public string FieldCaption { get; set; }
        public FieldArea FieldArea { get; set; }
        public int AreaIndex { get; set; }
        public FieldGroupInterval Interval { get; set; }
        public string GroupName { get; set; }
        public int GroupIndex { get; set; }
    }

    public class FieldGroup
    {
        public string GroupName { get; set; }
    }

    public class FieldTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Field field = (Field)item;
            if (field.Interval == FieldGroupInterval.DateMonthYear || field.Interval == FieldGroupInterval.DateQuarterYear 
                || field.Interval == FieldGroupInterval.DateMonth || field.Interval == FieldGroupInterval.DateYear)
            {
                return (DataTemplate)((Control)container).FindResource("IntervalFieldTemplate");
            }
            return (DataTemplate)((Control)container).FindResource("DefaultFieldTemplate");
        }
    }

}
