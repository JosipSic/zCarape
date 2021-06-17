using DevExpress.Xpf.PivotGrid;
using Prism.Commands;
using Prism.Mvvm;
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
    public class PivotViewModel : BindableBase
    {
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

            DataSource = _dbService.GetStavkeZaIzvestaj();
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
