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
using System.Threading.Tasks;
using Prism.Services.Dialogs;

namespace Proizvodnja.ViewModels
{
    public class MasineURaduViewModel : BindableBase, IRegionMemberLifetime
    {

        #region Fields

        private readonly IDbService _dbService;
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;

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
            if (param != null)
            {
                var values = (object[])param;
                var slika = (string)values[0];
                var nalog = (NalogURadu)values[1];

                Jezgro.Views.Slika s = new Jezgro.Views.Slika(slika, nalog.ArtikalSifra + " " + nalog.ArtikalNaziv, nalog.ArtikalDezen);
                s.Show();
            }
        }

        // EditNalogCommand
        private DelegateCommand<long?> _editNalogCommand;
        public DelegateCommand<long?> EditNalogCommand =>
            _editNalogCommand ?? (_editNalogCommand = new DelegateCommand<long?>(ExecuteEditNalogCommand));

        void ExecuteEditNalogCommand(long? parameter)
        {
            if (parameter == null || parameter == 0)
            {
                return;
            }
            NavigationParameters param = new NavigationParameters();
            param.Add("RadniNalogID", parameter);
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.NoviRN3, param);
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

        // ZakljuciNalogCommand
        private DelegateCommand<long?> _zakljuciNalogCommand;
        public DelegateCommand<long?> ZakljuciNalogCommand =>
            _zakljuciNalogCommand ?? (_zakljuciNalogCommand = new DelegateCommand<long?>(ExecuteZakljuciNalog));

        void ExecuteZakljuciNalog(long? radniNalogID)
        {
            if (radniNalogID == null || radniNalogID == 0)
            {
                return;
            }
            // Trazim kod kojih se sve masina javlja ovaj radniNalog
            var masineSaNalogom =
                from m in MasineURadu
                from z in m.Zadaci
                where z.NalogURadu.RadniNalogID == radniNalogID
                select m.MasinaNaziv;

            if (masineSaNalogom.Count() == 0)
            {
                return;
            }

            string poruka = "Da li ste sigurni?";
            if (masineSaNalogom.Count() > 1)
            {
                string porukaPref = "Izabrani nalog je aktivan na mašinama ";
                foreach (var item in masineSaNalogom)
                {
                    porukaPref += item + "; ";
                }
                porukaPref += "\nZaključenjem radni nalog se uklanja sa svih mašina.\n";
                poruka = porukaPref + poruka;
            }

            if (MessageBox.Show(poruka, "Zaključenje radnog naloga br." + radniNalogID.ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                == MessageBoxResult.No)
            {
                return;
            }

            _dbService.ZakljuciRadniNalog((long)radniNalogID);
            OsveziFormu();
        }

        // AktivirajNalogCommand
        private DelegateCommand<MasinaURadu> _aktivirajNalogCommand;
        public DelegateCommand<MasinaURadu> AktivirajNalogCommand =>
            _aktivirajNalogCommand ?? (_aktivirajNalogCommand = new DelegateCommand<MasinaURadu>(ExecuteAktivirajNalogCommand));

        void ExecuteAktivirajNalogCommand(MasinaURadu radniNalogID)
        {
            _dbService.AktivirajRadniNalog(radniNalogID.Istorija.RadniNalogID);
            OsveziFormu();
        }

        // DatumChangedCommand
        private DelegateCommand<Zadatak> _datumChangedCommand;
        public DelegateCommand<Zadatak> DatumChangedCommand =>
            _datumChangedCommand ?? (_datumChangedCommand = new DelegateCommand<Zadatak>(ExecuteDatumChangedCommand));

        void ExecuteDatumChangedCommand(Zadatak zadatak)
        {
            OsveziPredajnice(zadatak);
        }

        // PretnodniDanCommand
        private DelegateCommand<Zadatak> _prethodniDanComman;
        public DelegateCommand<Zadatak> PrethodniDanCommand =>
            _prethodniDanComman ?? (_prethodniDanComman = new DelegateCommand<Zadatak>(ExecutePrethodniDanCommand));

        void ExecutePrethodniDanCommand(Zadatak zadatak)
        {
            zadatak.DatumPredajnice = zadatak.DatumPredajnice.AddDays(-1);
            ExecuteDatumChangedCommand(zadatak);
        }

        // NaredniDanCommand
        private DelegateCommand<Zadatak> _naredniDanCommand;
        public DelegateCommand<Zadatak> NaredniDanCommand =>
            _naredniDanCommand ?? (_naredniDanCommand = new DelegateCommand<Zadatak>(ExecuteNaredniDanCommand));

        void ExecuteNaredniDanCommand(Zadatak zadatak)
        {
            if (zadatak.DatumPredajnice == DateTime.Now.Date)
            {
                return;
            }

            zadatak.DatumPredajnice = zadatak.DatumPredajnice.AddDays(1);
            ExecuteDatumChangedCommand(zadatak);
        }

        // PrethodniIstorijaCommand
        private DelegateCommand<MasinaURadu> _prethodniIstorijaCommand;
        public DelegateCommand<MasinaURadu> PrethodniIstorijaCommand =>
            _prethodniIstorijaCommand ?? (_prethodniIstorijaCommand = new DelegateCommand<MasinaURadu>(ExecutePrethodniIstorijaCommand));

        void ExecutePrethodniIstorijaCommand(MasinaURadu parameter)
        {
            FormirajIstoriju(parameter, Kretanje.Nazad);
        }

        // NaredniIstorijaCommand
        private DelegateCommand<MasinaURadu> _naredniIstorijaCommand;
        public DelegateCommand<MasinaURadu> NaredniIstorijaCommand =>
            _naredniIstorijaCommand ?? (_naredniIstorijaCommand = new DelegateCommand<MasinaURadu>(ExecuteNaredniIstorijaCommand));

        void ExecuteNaredniIstorijaCommand(MasinaURadu parameter)
        {
            FormirajIstoriju(parameter, Kretanje.Napred);
        }

        // MoveLeftCommand
        private DelegateCommand<Zadatak> _moveLeftCommand;
        public DelegateCommand<Zadatak> MoveLeftCommand =>
            _moveLeftCommand ?? (_moveLeftCommand = new DelegateCommand<Zadatak>(ExecuteMoveLeftCommand));

        void ExecuteMoveLeftCommand(Zadatak parameter)
        {
            MoveLeftZadatak(parameter);
        }

        // PregledRnCommand
        private DelegateCommand<long?> _pregledRnCommand;
        public DelegateCommand<long?> PregledRnCommand =>
            _pregledRnCommand ?? (_pregledRnCommand = new DelegateCommand<long?>(ExecutePregledRnCommand));

        void ExecutePregledRnCommand(long? parameter)
        {
            if (parameter == null || parameter == 0)
            {
                return;
            }
            IDialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("ID", parameter);
            _dialogService.Show("PregledRnDijalog", dialogParameters, null);
        }

        #endregion //Commands

        #region Ctor

        public MasineURaduViewModel(IDbService dbService, IRegionManager regionManager, IDialogService dialogService)
        {
            _dbService = dbService;
            _regionManager = regionManager;
            _dialogService = dialogService;
            FormirajListuRadnika();
            FormirajSpisakMasinaURadu();
            OsveziFormuUOdredjenoVreme();
        }


        #endregion //Ctor

        #region Methods

        private void OsveziFormu()
        {
            FormirajSpisakMasinaURadu();
        }

        /// <summary>
        /// Ponovo popunjava tabelu predatih kolicina po smenama za prosledjeni zadatak na dan u zadatku
        /// </summary>
        /// <param name="zadatak"></param>
        private void OsveziPredajnice(Zadatak zadatak)
        {
            IEnumerable<Predajnica> predajniceNaDan =
                _dbService.GetPredajniceByMasinaAndNalogOnDate(zadatak.MasinaID, zadatak.NalogURadu.RadniNalogID, zadatak.DatumPredajnice);

            Predajnica p;
            p = predajniceNaDan.FirstOrDefault(p => p.Smena == 1);
            if (p != null)
            {
                zadatak.PrvaSmenaPredajnicaID = p.ID;
                zadatak.PrvaSmenaRadnikID = p.LiceID;
                zadatak.PrvaSmena1kl = p.Kolicina;
                zadatak.PrvaSmena2kl = p.DrugaKl;
            }
            else
            {
                zadatak.PrvaSmenaPredajnicaID = 0;
                zadatak.PrvaSmenaRadnikID = 0;
                zadatak.PrvaSmena1kl = 0;
                zadatak.PrvaSmena2kl = 0;
            }

            p = predajniceNaDan.FirstOrDefault(p => p.Smena == 2);
            if (p != null)
            {
                zadatak.DrugaSmenaPredajnicaID = p.ID;
                zadatak.DrugaSmenaRadnikID = p.LiceID;
                zadatak.DrugaSmena1kl = p.Kolicina;
                zadatak.DrugaSmena2kl = p.DrugaKl;
            }
            else
            {
                zadatak.DrugaSmenaPredajnicaID = 0;
                zadatak.DrugaSmenaRadnikID = 0;
                zadatak.DrugaSmena1kl = 0;
                zadatak.DrugaSmena2kl = 0;
            }

            p = predajniceNaDan.FirstOrDefault(p => p.Smena == 3);
            if (p != null)
            {
                zadatak.TrecaSmenaPredajnicaID = p.ID;
                zadatak.TrecaSmenaRadnikID = p.LiceID;
                zadatak.TrecaSmena1kl = p.Kolicina;
                zadatak.TrecaSmena2kl = p.DrugaKl;
            }
            else
            {
                zadatak.TrecaSmenaPredajnicaID = 0;
                zadatak.TrecaSmenaRadnikID = 0;
                zadatak.TrecaSmena1kl = 0;
                zadatak.TrecaSmena2kl = 0;
            }

        }

        private void OsveziFormuUOdredjenoVreme()
        {
            var DailyTime = "14:12:01";
            var timeParts = DailyTime.Split(new char[1] { ':' });

            var dateNow = DateTime.Now;
            var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day,
                       int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));
            TimeSpan ts;
            if (date > dateNow)
                ts = date - dateNow;
            else
            {
                date = date.AddDays(1);
                ts = date - dateNow;
            }

            //waits certan time and run the code
            Task.Delay(ts).ContinueWith((x) => OsveziFormu());
        }


        private void FormirajSpisakMasinaURadu()
        {
            MasineURadu = new ObservableCollection<MasinaURadu>(_dbService.GetAllMasineURadu(DateTime.Now.Date));
            FormirajIstoriju();
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
            if (predajnicaID == 0 && kolicina == 0 && drugaKl == 0)
            {
                return;
            }

            // Ako nije izabrano lice ne snimam. Ne bi trebalo nikad da se desi 
            // posto unos kolicina u formi za unos nije omogucen ukoliko nije izabran radnik
            if (liceID == 0)
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
            if (azuriranaPredajnicaID == 0)
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

                if (changedProperty.EndsWith("1kl"))
                {
                    AzurirajPredatuKolicinu(zadatak.NalogURadu);
                }

            }
        }

        private void AzurirajPredatuKolicinu(NalogURadu nalogURadu)
        {
            nalogURadu.Uradjeno = _dbService.GetPredatoByRadniNalog(nalogURadu.RadniNalogID);
        }

        private void FormirajIstoriju(MasinaURadu masinaURadu = null, Kretanje kretanje = Kretanje.Nazad)
        {
            if (masinaURadu == null)
            {
                foreach (MasinaURadu item in MasineURadu)
                {
                    FormirajIstorijuIns(item, kretanje);
                }
            }
            else
            {
                FormirajIstorijuIns(masinaURadu, kretanje);
            }
        }

        private void FormirajIstorijuIns(MasinaURadu masinaURadu, Kretanje kretanje)
        {
            long trenutniRnID = masinaURadu.Istorija == null ? 0 : masinaURadu.Istorija.RadniNalogID;
            var istorijaTemp = _dbService.GetNextIstorija(masinaURadu.MasinaID, trenutniRnID, kretanje);
            if (istorijaTemp != null)
            {
                masinaURadu.Istorija = istorijaTemp;
            }
        }

        private void MoveLeftZadatak(Zadatak zadatak2)
        {
            if (zadatak2 == null) return;

            // Zadatak koji je istog statusa i iste hitnosti a ima redosled koji je najveci, ali manji ili jednak od zadatak2.redosled
            // ako je isti redosled onda gleda najveci RadniNalogID
            var zadaciIspred = (from m in MasineURadu
                                where m.MasinaID == zadatak2.MasinaID
                                from z in m.Zadaci
                                where z.NalogURadu.StatusNaloga == zadatak2.NalogURadu.StatusNaloga
                                && z.Hitno == zadatak2.Hitno
                                && (z.Redosled < zadatak2.Redosled || (z.Redosled==zadatak2.Redosled && z.ID<zadatak2.ID))
                               && z.ID != zadatak2.ID
                               orderby z.Redosled descending
                               select z).ToList();
            if (zadaciIspred == null || zadaciIspred.Count == 0)
                return;

            int redosledPrethodnogZadatka = zadaciIspred.First().Redosled;
            int zahtevaniRedosled = redosledPrethodnogZadatka - 1;
            _dbService.SetRedosledAngazovaneMasine(zadatak2.ID, redosledPrethodnogZadatka - 1);

            // Ako ima vise zadataka promeravam redosled svih prethodnih
            if (zadaciIspred.Count > 1)
            {
                for (int i = 1; i < zadaciIspred.Count; i++)
                {
                    if (zadaciIspred[i].Redosled >= zahtevaniRedosled)
                    {
                        zahtevaniRedosled--;
                        _dbService.SetRedosledAngazovaneMasine(zadaciIspred[i].ID, zahtevaniRedosled);
                    }
                }
            }



            //Zadatak zadatak1 = (from m in MasineURadu
            //                    where m.MasinaID == zadatak2.MasinaID
            //                    from z in m.Zadaci
            //                    where z.NalogURadu.StatusNaloga == zadatak2.NalogURadu.StatusNaloga 
            //                    && z.Hitno == zadatak2.Hitno
            //                    && z.Redosled <= zadatak2.Redosled 
            //                    && z.ID!=zadatak2.ID
            //                    orderby z.Redosled descending
            //                    select z).FirstOrDefault();
            //if (zadatak1 == null) return;

            OsveziFormu();
        }


        #endregion //Methods

        #region IRegionMemberLifetime
        public bool KeepAlive => false;

        #endregion //IRegionMemberLifetime

    }
}
