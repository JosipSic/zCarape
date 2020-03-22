using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using zCarape.Core;
using zCarape.Core.Models;

namespace Jezgro.ViewModels
{
    public class PromeniOznakuVelicineViewModel : BindableBase
    {
        private long id = 0;
        
        private string _staraOznaka;
        public string StaraOznaka
        {
            get { return _staraOznaka; }
            set { SetProperty(ref _staraOznaka, value); }
        }

        private string _novaOznaka;
        public string NovaOznaka
        {
            get { return _novaOznaka; }
            set { SetProperty(ref _novaOznaka, value); }
        }

        private DelegateCommand _odustaniCommand;
        public DelegateCommand OdustaniCommand =>
            _odustaniCommand ?? (_odustaniCommand = new DelegateCommand(ExecuteOdustaniCommand));

        void ExecuteOdustaniCommand()
        {
        }

        public PromeniOznakuVelicineViewModel(Velicina velicina)
        {
            if (velicina!=null)
            {
                StaraOznaka = velicina.Oznaka;
                id = velicina.ID;
            }
        }


    }
}
