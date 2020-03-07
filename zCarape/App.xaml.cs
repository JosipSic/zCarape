using Prism.Ioc;
using zCarape.Views;
using System.Windows;
using Prism.Modularity;
using Proizvodnja;
using Jezgro;
using zCarape.ViewModels;
using Prism.Mvvm;
using zCarape.Services.Interfaces;
using zCarape.Services;

namespace zCarape
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            StartUpCode.StartUp();
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();

            // Servisi
            containerRegistry.RegisterSingleton<IDbService, DBService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<JezgroModule>();
            moduleCatalog.AddModule<ProizvodnjaModule>();
        }
    }
}
