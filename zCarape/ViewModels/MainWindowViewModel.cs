using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Proizvodnja.Views;
using zCarape.Core;

namespace zCarape.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly IRegionManager _regionManager;

        #endregion //Fields

        #region Properties

        private string _title = "zSocks";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        #endregion //Properties

        #region Cmd

        // NavigateCommand
        private DelegateCommand<string> _navigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));

        void ExecuteNavigateCommand(string navigationPath)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, navigationPath);
        }

        #endregion //Cmd

        #region Ctor

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            //ExecuteNavigateCommand(ViewNames.MasineURadu); Ovaj kod ne radi
            regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(MasineURadu));
        }

        #endregion //Ctor
    }
}