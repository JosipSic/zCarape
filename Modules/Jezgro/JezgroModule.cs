using Jezgro.ViewModels;
using Jezgro.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using zCarape.Core;

namespace Jezgro
{
    public class JezgroModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public JezgroModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.DezeniRegion, typeof(Dezeni));
        }


        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Registracija za navigaciju
            containerRegistry.RegisterForNavigation<Artikli, ArtikliViewModel>();
            containerRegistry.RegisterForNavigation<Velicine, VelicineViewModel>();
            containerRegistry.RegisterForNavigation<ArtikliEdit, ArtikliEditViewModel>();

            // Za svaki View vezujem odgovarajuci ViewModel (kod radi i bez toga posto sledi Prizm konvenziju, ali je ovako brze posto nema potrebe da se koristi refleksija
            ViewModelLocationProvider.Register<Artikli, ArtikliViewModel>();
            ViewModelLocationProvider.Register<Velicine, VelicineViewModel>();
            ViewModelLocationProvider.Register<ArtikliEdit, ArtikliEditViewModel>();
        }
    }
}