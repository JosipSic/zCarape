using DevExpress.Mvvm.DataAnnotations;
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
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class MasineViewModel : BindableBase, IRegionMemberLifetime
    {
        private readonly IDbService _dbService;

        #region Properties
        // Masine
        private ObservableCollection<Masina> _masine;
        public ObservableCollection<Masina> Masine
        {
            get { return _masine; }
            set { SetProperty(ref _masine, value); }
        }

        // SelectedMasina
        private Masina _selectedMasina;
        public Masina SelectedMasina
        {
            get { return _selectedMasina; }
            set { SetProperty(ref _selectedMasina, value); }
        }

        // KeepAlive
        public bool KeepAlive => false;
        #endregion

        #region Ctor
        public MasineViewModel(IDbService dbService)
        {
            _dbService = dbService;
            FormirajSpisakMasina();
        }
        #endregion

        #region Command
        private DelegateCommand<DevExpress.Xpf.Grid.GridRowValidationEventArgs> _validateRowCommand;
        public DelegateCommand<DevExpress.Xpf.Grid.GridRowValidationEventArgs> ValidateRowCommand =>
            _validateRowCommand ?? (_validateRowCommand = new DelegateCommand<DevExpress.Xpf.Grid.GridRowValidationEventArgs>(ValidateRow));

        public void ValidateRow(DevExpress.Xpf.Grid.GridRowValidationEventArgs e)
        {
            if (SelectedMasina == null)
                return;

            string greska = "";

            if (string.IsNullOrWhiteSpace(SelectedMasina.Naziv))
            {
                greska += "Niste uneli oznaku mašine.";
            }

            if (!string.IsNullOrEmpty(greska))
            {
                e.IsValid = false;
                e.ErrorContent = greska;
                return;
            }

            if (SelectedMasina.ID==0)
            {
                SelectedMasina.Aktivan = true;
            }

            long odgovor = _dbService.InsertOrUpdateMasina(SelectedMasina);

            if (odgovor==0)
            {
                e.IsValid = false;
                e.ErrorContent = "ZapisNijeSnimljen";
                return;
            }
            else if (SelectedMasina.ID==0 && SelectedMasina.ID!=odgovor)
            {
                SelectedMasina.ID = odgovor;
            }
        }

        private DelegateCommand izbrisiMasinuCommand;
        public ICommand IzbrisiMasinuCommand => izbrisiMasinuCommand ??= new DelegateCommand(IzbrisiMasinu);
        public void IzbrisiMasinu()
        {
            if (SelectedMasina == null)
                return;
            
            long idZaBrisanje = SelectedMasina.ID;

            if (idZaBrisanje == 0)
                return;

            if (_dbService.MasinaImaZavisneZapise(id: idZaBrisanje))
            {
                DXMessageBox.Show("Postoje zavisni zapisi. Ukoliko se mašina više ne koristi obeležite da je neaktivna", "Nije dozvoljeno brisanje", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_dbService.DeleteMasina(id: idZaBrisanje))
            {
               this.Masine.Remove(SelectedMasina);
            }
        }
        #endregion

        #region Methods
        private void FormirajSpisakMasina()
        {
            Masine = new ObservableCollection<Masina>(_dbService.GetAllMasine());
        }


        #endregion

    }
}
