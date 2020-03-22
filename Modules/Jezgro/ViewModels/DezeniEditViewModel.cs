using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using zCarape.Core;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Jezgro.ViewModels
{
    public class DezeniEditViewModel : BindableBase
    {

        #region Fields
        private readonly IDbService _dbService;
        private long _dezenArtiklaID;
        private long _artikalID;
        #endregion //Fields

        #region Properties
        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set { SetProperty(ref _caption, value); }
        }

        //Naziv
        private string _naziv;
        public string Naziv
        {
            get { return _naziv; }
            set { SetProperty(ref _naziv, value); }
        }

        //Opis
        private string _opis;
        public string Opis
        {
            get { return _opis; }
            set { SetProperty(ref _opis, value); }
        }

        //Putanja
        private string _putanja;
        public string Putanja
        {
            get { return _putanja; }
            set { SetProperty(ref _putanja, value); }
        }

        //Slika1
        private string _slika1;
        public string Slika1
        {
            get { return _slika1; }
            set { SetProperty(ref _slika1, value); UkloniSlikuCommand.RaiseCanExecuteChanged(); }
        }

        //Slika2
        private string _slika2;
        public string Slika2
        {
            get { return _slika2; }
            set { SetProperty(ref _slika2, value); UkloniSlikuCommand.RaiseCanExecuteChanged(); }
        }

        //Slika3
        private string _slika3;
        public string Slika3
        {
            get { return _slika3; }
            set { SetProperty(ref _slika3, value); UkloniSlikuCommand.RaiseCanExecuteChanged(); }
        }
        #endregion //Properties

        #region Commands
        //DodajSlikuCommand
        private DelegateCommand<string> _dodajSlikuCommand;
        public DelegateCommand<string> DodajSlikuCommand =>
            _dodajSlikuCommand ?? (_dodajSlikuCommand = new DelegateCommand<string>(ExecuteDodajSlikuCommand));

        void ExecuteDodajSlikuCommand(string parameter)
        {
            // 1-Dijalog za lociranje slike
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Slike (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = GlobalniKod.SlikeDir;
            string folder, imeSlike;
            if (openFileDialog.ShowDialog() == true)
            {
                folder = Path.GetDirectoryName(openFileDialog.FileName);
                imeSlike = Path.GetFileName(openFileDialog.FileName);
            }
            else
            {
                return;
            }

            // 2-Ako je slika locirana proveravam da li se nalazu u Slike direktorijumu, ako ne kopiram je tu
            if (folder != GlobalniKod.SlikeDir)
            {
                int dodatak = 1;
                string novoIme = imeSlike;
                while (File.Exists(Path.Combine(GlobalniKod.SlikeDir, novoIme)))
                {
                    string imeSlikeBezEkstenzije = Path.GetFileNameWithoutExtension(imeSlike);
                    string ekstenzija = Path.GetExtension(imeSlike);
                    novoIme = imeSlikeBezEkstenzije + string.Format("_{0}", ++dodatak) + ekstenzija ;
                }

                File.Copy(openFileDialog.FileName, Path.Combine(GlobalniKod.SlikeDir, novoIme));
                imeSlike = novoIme;
            }

            // 3-Dodajem sliku svojstvu
            switch (parameter)
            {
                case "1":
                    this.Slika1 = imeSlike;
                    break;
                case "2":
                    this.Slika2 = imeSlike;
                    break;
                case "3":
                    this.Slika3 = imeSlike;
                    break;
                 default:
                    break;
            }
        }

        //UkloniSlikuCommand
        private DelegateCommand<string> _ukloniSlukuCommand;
        public DelegateCommand<string> UkloniSlikuCommand =>
            _ukloniSlukuCommand ?? (_ukloniSlukuCommand = new DelegateCommand<string>(ExecuteUkloniSlikuCommand, CanExecuteUkloniSlikuCommand));

        void ExecuteUkloniSlikuCommand(string parameter)
        {
            switch (parameter)
            {
                case "1":
                    Slika1 = string.Empty;
                    break;
                case "2":
                    Slika2 = string.Empty;
                    break;
                case "3":
                    Slika3 = string.Empty;
                    break;
                default:
                    break;
            }
        }

        bool CanExecuteUkloniSlikuCommand(string parameter)
        {
            switch (parameter)
            {
                case "1":
                    return !string.IsNullOrWhiteSpace(Slika1);
                case "2":
                    return !string.IsNullOrWhiteSpace(Slika2);
                case "3":
                    return !string.IsNullOrWhiteSpace(Slika3);
                default:
                    return true;
            }
        }

        //SnimiCommand
        private DelegateCommand _snimiCommand;
        public DelegateCommand SnimiCommand =>
            _snimiCommand ?? (_snimiCommand = new DelegateCommand(ExecuteSnimiCommand));

        void ExecuteSnimiCommand()
        {
            if (!UslovZaUpis())
                return;

            long odgovor = _dbService.InsertOrUpdateDezenArtikla(new DezenArtikla()
            {
                ID = this._dezenArtiklaID,
                ArtikalID = this._artikalID,
                Naziv = this.Naziv,
                Opis = this.Opis,
                Putanja = this.Putanja,
                Slika1 = this.Slika1,
                Slika2 = this.Slika2,
                Slika3 = this.Slika3,
                Aktivan = true
            }) ;
            if (odgovor > 0)
            {
               _dezenArtiklaID = odgovor;
                GlobalniKod.DezenParam.VracenSnimljen = true;
                ZatvoriFormu();
            }
        }

        //OdustaniCommand
        private DelegateCommand _odustaniCommand;

        public DelegateCommand OdustaniCommand =>
            _odustaniCommand ?? (_odustaniCommand = new DelegateCommand(ExecuteOdustaniCommand));


        void ExecuteOdustaniCommand()
        {
            GlobalniKod.DezenParam.VracenBezPromene = true;
            ZatvoriFormu();
        }

        //Izbrisi Command
        private DelegateCommand _izbrisiCommand;
        public DelegateCommand IzbrisiCommand =>
            _izbrisiCommand ?? (_izbrisiCommand = new DelegateCommand(ExecuteIzbrisiCommand,CanExecuteIzbrisiCommand));

        private bool CanExecuteIzbrisiCommand()
        {
            return this._dezenArtiklaID>0;
        }

        void ExecuteIzbrisiCommand()
        {
            if (this._dezenArtiklaID==0)
            {
                return;
            }

            if (MessageBox.Show("Da li ste sigurni da želite da izbrišete ovaj dezen?","Brisanje",MessageBoxButton.YesNo,MessageBoxImage.Question,MessageBoxResult.No)
                ==MessageBoxResult.Yes)
            {
                if (_dbService.IzbrisiDezenArtikla(this._dezenArtiklaID))
                {
                    GlobalniKod.DezenParam.VracenIzbrisan = true;
                    ZatvoriFormu();
                }
            }
        }

        #endregion //Commands

        #region Methods


        private bool UslovZaUpis()
        {
            if (string.IsNullOrWhiteSpace(Naziv))
            {
                MessageBox.Show("Polje \"Naziv\" je obavezno.");
            }

            return _artikalID > 0 && !string.IsNullOrWhiteSpace(Naziv);
        }

        private void ZatvoriFormu()
        {
            if (GlobalniKod.DezenParam.Window!=null)
            {
                GlobalniKod.DezenParam.Window.Close();
            }
        }

        private void ProcitajDezenParam()
        {
            if (GlobalniKod.DezenParam == null || GlobalniKod.DezenParam.ArtikalID == 0 || string.IsNullOrWhiteSpace(GlobalniKod.DezenParam.ArtikalSifra))
            {
                MessageBox.Show("Nije prosledjen parametar ArtikalID. Nije moguce otvoriti formu za editovanje dezena.");
                ZatvoriFormu();
                return;
            }

            _dezenArtiklaID = GlobalniKod.DezenParam.DezenArtiklaID;
            _artikalID = GlobalniKod.DezenParam.ArtikalID;
            string artikalSifra = GlobalniKod.DezenParam.ArtikalSifra;
            string artikalNaziv = GlobalniKod.DezenParam.ArtikalNaziv;


            if (_dezenArtiklaID == 0)
            {
                // Novi unos
                this.Caption = String.Format($"NOVI DEZEN za artikal {artikalSifra} {artikalNaziv}");
            }
            else
            {
                // Editovanje postojeceg Dezena
                if (UcitajDezenArtikla(_dezenArtiklaID))
                {
                    this.Caption = String.Format($"{artikalSifra} {artikalNaziv} - Dezen: {this.Naziv}");
                }
                else
                {
                    MessageBox.Show("Nije moguce ucitati iz baze trazeni dezen.");
                    ZatvoriFormu();
                    return;
                }
            }

            IzbrisiCommand.RaiseCanExecuteChanged();
        }


        private bool UcitajDezenArtikla(long id)
        {
            DezenArtikla dezenArtikla = _dbService.GetDezenArtikla(id);
            if (dezenArtikla!=null)
            {
                this._dezenArtiklaID = dezenArtikla.ID;
                this._artikalID = dezenArtikla.ArtikalID;
                this.Naziv = dezenArtikla.Naziv;
                this.Opis = dezenArtikla.Opis;
                this.Putanja = dezenArtikla.Putanja;
                this.Slika1 = dezenArtikla.Slika1;
                this.Slika2 = dezenArtikla.Slika2;
                this.Slika3 = dezenArtikla.Slika3;
                return true;
            }
            return false;
        }

        #endregion //Methods

        #region Ctor
        public DezeniEditViewModel(IDbService dbService)
        {
            _dbService = dbService;
            ProcitajDezenParam();
        }

        #endregion //Ctor

    }
}