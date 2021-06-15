using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using zCarape.Core.Business;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace Proizvodnja.ViewModels
{
    public class PregledRnDijalogViewModel : BindableBase, IDialogAware
    {
        #region Fields

        private readonly IDbService _dbService;
        private long _radniNalogID;

        #endregion //Fields

        #region Properties

        private RadniNalogPregled _nalog;
        public RadniNalogPregled Nalog
        {
            get { return _nalog; }
            set { SetProperty(ref _nalog, value); }
        }

        private IEnumerable<PredajnicaPregled> _predajnice;
        public IEnumerable<PredajnicaPregled> Predajnice
        {
            get { return _predajnice; }
            set { SetProperty(ref _predajnice, value); }
        }

        #endregion //Properties

        #region Commands
        // CloseDialogCommand
        private DelegateCommand _closeDialogCommand;

        public DelegateCommand CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand(ExecuteCloseDialogCommand));

        void ExecuteCloseDialogCommand()
        {
            RequestClose?.Invoke(null);
        }

        // ExcelCommand
        private DelegateCommand _excelCommand;
        public DelegateCommand ExcelCommand =>
            _excelCommand ?? (_excelCommand = new DelegateCommand(ExecuteExcelCommand));

        void ExecuteExcelCommand()
        {

        }
        #endregion //Commands

        #region ctor
        public PregledRnDijalogViewModel(IDbService dbService)
        {
            _dbService = dbService;
        }

        #endregion //Ctor

        #region Methods

        private void PopuniPodatke()
        {
            Nalog = _dbService.GetRadniNalogPregled(_radniNalogID);
            Predajnice = Nalog.Predajnice;
        }

        #endregion //Methods

        #region IDialogAware
        public string Title => "Radni nalog " + _radniNalogID.ToString();

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            _radniNalogID = parameters.GetValue<long>("ID");
            PopuniPodatke();
        }
        #endregion //IDialogAware
    }
}