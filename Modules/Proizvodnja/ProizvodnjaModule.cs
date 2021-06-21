using Proizvodnja.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Proizvodnja.ViewModels;
using Prism.Mvvm;
using zCarape.Services.Interfaces;
using zCarape.Services;

namespace Proizvodnja
{
    public class ProizvodnjaModule : IModule
    {

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Registracija za navigaciju
            containerRegistry.RegisterForNavigation<MasineURadu, MasineURaduViewModel>();
            containerRegistry.RegisterForNavigation<Masine, MasineViewModel>();
            containerRegistry.RegisterForNavigation<Izvestaji, IzvestajiViewModel>();
            containerRegistry.RegisterForNavigation<Pivot, PivotViewModel>();
            containerRegistry.RegisterForNavigation<Predajnica, PredajnicaViewModel>();
            containerRegistry.RegisterForNavigation<NoviRN1Artikal, NoviRN1ArtikalViewModel>();
            containerRegistry.RegisterForNavigation<NoviRN2Dezen, NoviRN2DezenViewModel>();
            containerRegistry.RegisterForNavigation<NoviRN3, NoviRN3ViewModel>();
            containerRegistry.RegisterForNavigation<Lica, LicaViewModel>();

            containerRegistry.RegisterDialog<PregledRnDijalog, PregledRnDijalogViewModel>();

            // Za svaki View vezujem odgovarajuci ViewModel (kod radi i bez toga posto sledi Prizm konvenziju, ali je ovako brze posto nema potrebe da se koristi refleksija
            ViewModelLocationProvider.Register<Proizvodnja.Views.MasineURadu, Proizvodnja.ViewModels.MasineURaduViewModel>();
            ViewModelLocationProvider.Register<Masine, MasineViewModel>();
            ViewModelLocationProvider.Register<Izvestaji, IzvestajiViewModel>();
            ViewModelLocationProvider.Register<Pivot, PivotViewModel>();
            ViewModelLocationProvider.Register<Predajnica, PredajnicaViewModel>();
            ViewModelLocationProvider.Register<NoviRN2Dezen, NoviRN2DezenViewModel>();
            ViewModelLocationProvider.Register<NoviRN3, NoviRN3ViewModel>();
            ViewModelLocationProvider.Register<Lica, LicaViewModel>();

            // Servisi
            // containerRegistry.RegisterSingleton<IDbService, DBService>(); //Ovaj servis se registruje u glavnom zCarapeProjektu
        }
    }
}