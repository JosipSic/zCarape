using DevExpress.Xpf.Core;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using zCarape.Core;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class LicaViewModel : BindableBase, IRegionMemberLifetime
    {
        private readonly IDbService _dbService;
        #region Properties
        // Lica
        private ObservableCollection<Lice> _lica;
        public ObservableCollection<Lice> Lica
        {
            get { return _lica; }
            set { SetProperty(ref _lica, value); }
        }

        // SelectedLice
        private Lice _selectedLice;

        public Lice SelectedLice
        {
            get { return _selectedLice; }
            set { SetProperty(ref _selectedLice, value); }
        }
        public bool KeepAlive => false;

        #endregion

        #region Ctor
        public LicaViewModel(IDbService dbService)
        {
            _dbService = dbService;
            FormirajSpisakLica();
        }

        #endregion

        #region Commands
        private DelegateCommand<DevExpress.Xpf.Grid.GridRowValidationEventArgs> _validateRowCommand;
        public DelegateCommand<DevExpress.Xpf.Grid.GridRowValidationEventArgs> ValidateRowCommand =>
            _validateRowCommand ?? (_validateRowCommand = new DelegateCommand<DevExpress.Xpf.Grid.GridRowValidationEventArgs>(ValidateRow));

        public void ValidateRow(DevExpress.Xpf.Grid.GridRowValidationEventArgs e)
        {
            if (SelectedLice == null)
                return;

            string greska = "";

            if (string.IsNullOrWhiteSpace(SelectedLice.Ime))
            {
                greska += "Niste uneli ime radnika.";
            }

            if (!string.IsNullOrEmpty(greska))
            {
                e.IsValid = false;
                e.ErrorContent = greska;
                return;
            }

            if (SelectedLice.ID == 0)
            {
                SelectedLice.Aktivan = true;
            }

            long odgovor = _dbService.InsertOrUpdateLice(SelectedLice);

            if (odgovor == 0)
            {
                e.IsValid = false;
                e.ErrorContent = "ZapisNijeSnimljen";
                return;
            }
            else if (SelectedLice.ID == 0 && SelectedLice.ID != odgovor)
            {
                SelectedLice.ID = odgovor;
            }

            GlobalniKod.RadniciSuAzurirani = true;
        }

        private DelegateCommand izbrisiLiceCommand;
        public ICommand IzbrisiLiceCommand => izbrisiLiceCommand ??= new DelegateCommand(IzbrisiLice);
        public void IzbrisiLice()
        {
            if (SelectedLice == null)
                return;

            long idZaBrisanje = SelectedLice.ID;

            if (idZaBrisanje == 0)
                return;

            if (_dbService.LiceImaZavisneZapise(id: idZaBrisanje))
            {
                DXMessageBox.Show("Postoje zavisni zapisi. Ukoliko lice vise nije u radnom odnosu oznacite da je naktivno", "Nije dozvoljeno brisanje", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_dbService.DeleteLice(id: idZaBrisanje))
            {
                this.Lica.Remove(SelectedLice);
            }
            GlobalniKod.RadniciSuAzurirani = true;
        }
        #endregion

        #region Methods
        private void FormirajSpisakLica()
        {
            Lica = new ObservableCollection<Lice>(_dbService.GetAllLica());
        }
        #endregion
    }
}
