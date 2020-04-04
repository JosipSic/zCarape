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
using System.Windows;

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

        // OldZadatak
        private Zadatak _oldZadatak;
        public Zadatak OldZadatak
        {
            get { return _oldZadatak; }
            set { SetProperty(ref _oldZadatak, value); }
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

        // GotKeyboardFocusCommand
        private DelegateCommand<Zadatak> _gotFocusCommand;
        public DelegateCommand<Zadatak> GotFocusCommand =>
            _gotFocusCommand ?? (_gotFocusCommand = new DelegateCommand<Zadatak>(ExecuteGotFocusCommand));

        void ExecuteGotFocusCommand(Zadatak parameter)
        {
            OldZadatak = (Zadatak)parameter.Clone();
        }

        // LostKeyboardFocusCommand
        private DelegateCommand<Zadatak> _lostFocusCommand;
        public DelegateCommand<Zadatak> LostFocusCommand =>
            _lostFocusCommand ?? (_lostFocusCommand = new DelegateCommand<Zadatak>(ExecuteLostFocusCommand));

        void ExecuteLostFocusCommand(Zadatak parameter)
        {
            string changedProperty = string.Empty;

            // ako zapis nije nov samo jedno svojstvo moze biti promenjeno
            if (OldZadatak.PrvaSmena1kl != parameter.PrvaSmena1kl)
                changedProperty = "PrvaSmena1kl";
            else if (OldZadatak.DrugaSmena1kl != parameter.DrugaSmena1kl)
                changedProperty = "DrugaSmena1kl";
            else if (OldZadatak.TrecaSmena1kl != parameter.TrecaSmena1kl)
                changedProperty = "TrecaSmena1kl";
            else if (OldZadatak.PrvaSmena2kl != parameter.PrvaSmena2kl)
                changedProperty = "PrvaSmena2kl";
            else if (OldZadatak.DrugaSmena2kl != parameter.DrugaSmena2kl)
                changedProperty = "DrugaSmena2kl";
            else if (OldZadatak.TrecaSmena2kl != parameter.TrecaSmena2kl)
                changedProperty = "TrecaSmena2kl";
            else if (OldZadatak.PrvaSmenaRadnikID != parameter.PrvaSmenaRadnikID)
                changedProperty = "PrvaSmenaRadnikID";
            else if (OldZadatak.DrugaSmenaRadnikID != parameter.DrugaSmenaRadnikID)
                changedProperty = "DrugaSmenaRadnikID";
            else if (OldZadatak.TrecaSmenaRadnikID != parameter.TrecaSmenaRadnikID)
                changedProperty = "TrecaSmenaRadnikID";

            if (!string.IsNullOrEmpty(changedProperty))
            {
                UpdatePredajnica(parameter, changedProperty);
            }
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
            MasineURadu = new ObservableCollection<MasinaURadu>(_dbService.GetAllMasineURadu(DateTime.Now.Date));
        }

        private void FormirajListuRadnika()
        {
            Radnici = new ObservableCollection<Lice>(_dbService.GetAllAktivnaLica());
        }

        private void UpdatePredajnica(Zadatak zadatak, string changedProperty)
        {
            long predajnicaID;
            long radniNalogID = zadatak.NalogURadu.RadniNalogID;
            long masinaID = zadatak.MasinaID;
            DateTime datum = zadatak.DatumPredajnice;
            long liceID;
            byte smena;
            long kolicina;
            long drugaKl;

            switch (changedProperty)
            {
                case "PrvaSmena1kl":
                case "PrvaSmena2kl":
                case "PrvaSmenaRadnikID":
                    smena = 1;
                    predajnicaID = zadatak.PrvaSmenaPredajnicaID;
                    kolicina = zadatak.PrvaSmena1kl;
                    drugaKl = zadatak.PrvaSmena2kl;
                    liceID = zadatak.PrvaSmenaRadnikID;
                    break;
                case "DrugaSmena1kl":
                case "DrugaSmena2kl":
                case "DrugaSmenaRadnikID":
                    smena = 2;
                    predajnicaID = zadatak.DrugaSmenaPredajnicaID;
                    kolicina = zadatak.DrugaSmena1kl;
                    drugaKl = zadatak.DrugaSmena2kl;
                    liceID = zadatak.DrugaSmenaRadnikID;
                    break;
                case "TrecaSmena1kl":
                case "TrecaSmena2kl":
                case "TrecaSmenaRadnikID":
                    smena = 3;
                    predajnicaID = zadatak.TrecaSmenaPredajnicaID;
                    kolicina = zadatak.TrecaSmena1kl;
                    drugaKl = zadatak.TrecaSmena2kl;
                    liceID = zadatak.TrecaSmenaRadnikID;
                    break;
                default:
                    throw new Exception("Parametar: " + changedProperty + " nije predvidjen.");
            }

            // Ako je zapis nov (nije snimljen), a izabrano je samo lice bez bez finansijskih iznosa onda ne snimam
            if (predajnicaID==0 && kolicina==0 && drugaKl==0)
            {
                return;
            }

            // Ako nije izabrano lice ne snimam. Ne bi trebalo nikad da se desi 
            // posto unos kolicina u formi za unos nije omogucen ukoliko nije izabran radnik
            if (liceID==0)
            {
                MessageBox.Show("Unos nije snimljen u bazu. Nije moguce snimiti zapis bez izabranog radnika", "Problem", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Predajnica predajnica = new Predajnica()
            {
                ID = predajnicaID,
                RadniNalogID = radniNalogID,
                MasinaID = masinaID,
                Datum = datum,
                LiceID = liceID,
                Smena = smena,
                Kolicina = kolicina,
                DrugaKl = drugaKl
            };

            string kolona;
            switch (changedProperty)
            {
                case "PrvaSmena1kl":
                case "DrugaSmena1kl":
                case "TrecaSmena1kl":
                    kolona = "kolicina";
                    break;
                case "PrvaSmena2kl":
                case "DrugaSmena2kl":
                case "TrecaSmena2kl":
                    kolona = "drugakl";
                    break;
                case "PrvaSmenaRadnikID":
                case "DrugaSmenaRadnikID":
                case "TrecaSmenaRadnikID":
                    kolona = "liceid";
                    break;
                default:
                    kolona = string.Empty;
                    break;
            }

            long azuriranaPredajnicaID = _dbService.InsertOrUpdatePredajnica(predajnica, kolona);
            if (azuriranaPredajnicaID==0)
            {
                MessageBox.Show("Baza nije azurirana.", "Problem", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                switch (smena)
                {
                    case 1:
                        zadatak.PrvaSmenaPredajnicaID = azuriranaPredajnicaID;
                        break;
                    case 2:
                        zadatak.DrugaSmenaPredajnicaID = azuriranaPredajnicaID;
                        break;
                    case 3:
                        zadatak.TrecaSmenaPredajnicaID = azuriranaPredajnicaID;
                        break;
                    default:
                        break;
                }

            }
        }

        #endregion //Methods

        #region IRegionMemberLifetime
        public bool KeepAlive => false;

        #endregion //IRegionMemberLifetime

    }
}
