using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace zCarape.Core
{
    public class DezenParam
    {
        public long DezenArtiklaID { get; set; }
        public long ArtikalID { get; set; }
        public string ArtikalSifra { get; set; }
        public string ArtikalNaziv { get; set; }
        public bool VracenSnimljen { get; set; }
        public bool VracenIzbrisan { get; set; }
        public bool VracenBezPromene { get; set; }
        public Window Window { get; set; }

        public DezenParam()
        {
            this.Blanko();
        }

        public void Blanko()
        {
            this.DezenArtiklaID = 0;
            this.ArtikalID = 0;
            this.ArtikalSifra = this.ArtikalNaziv = string.Empty;
            this.VracenSnimljen = this.VracenIzbrisan = this.VracenBezPromene = false;
            this.Window = null;
        }
    }
}
