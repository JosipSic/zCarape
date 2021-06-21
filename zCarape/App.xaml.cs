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
using DevExpress.Xpf.Core;
using DevExpress.Mvvm;
using System;

namespace zCarape
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            SplashScreenManager.CreateFluent(new DXSplashScreenViewModel()
            {
                Copyright = "www.zajo.co.rs",
                IsIndeterminate = true,
                Logo = new Uri(@"\Images\Detelina.png", uriKind: UriKind.Relative),
                Status = "Pokrećem aplikaciju ...",
                Title = "zSocks",
                Subtitle = "Praćenje toka proizvodnje"
            }
            ).ShowOnStartup();
        }

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
