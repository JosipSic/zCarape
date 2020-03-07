using Jezgro.ViewModels;
using Jezgro.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Jezgro
{
    public class JezgroModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Registracija za navigaciju
            containerRegistry.RegisterForNavigation<Artikli, ArtikliViewModel>();

            // Za svaki View vezujem odgovarajuci ViewModel (kod radi i bez toga posto sledi Prizm konvenziju, ali je ovako brze posto nema potrebe da se koristi refleksija
            ViewModelLocationProvider.Register<Artikli, ArtikliViewModel>();
        }
    }
}