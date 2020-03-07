using System;
using System.Collections.Generic;
using System.Text;
using zCarape.Core.Models;

namespace zCarape.Services.Interfaces
{
    public interface IDbService
    {
        bool TestConnection(out string poruka);

        #region Masine
        IEnumerable<Masina> GetAllMasine();

        Masina GetByNazivMasina(string naziv);

        long InsertOrUpdateMasina(Masina masina);

        bool MasinaImaZavisneZapise(long id);

        bool DeleteMasina(long id);

        IEnumerable<MasinaURadu> GetAllMasineURadu();
        #endregion //Masine

        #region Artikli
        IEnumerable<Artikal> GetAllArtikli();

        IEnumerable<Velicina> GetVelicineArtikla(long artikalID);

        IEnumerable<DezenArtikla> GetDezeniArtikla(long artikalID);

        long InsertOrUpdateArtikal(Artikal artikal);

        #endregion //Artikli
    }
}
