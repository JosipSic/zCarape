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

        Artikal GetArtikal(long artikalID);

        IEnumerable<Artikal> GetAllArtikli();

        long InsertOrUpdateArtikal(Artikal artikal);


        #endregion //Artikli

        #region DezeniArtikala
        DezenArtikla GetDezenArtikla(long dezenArtiklaID);

        long InsertOrUpdateDezenArtikla(DezenArtikla dezenArtikla);

        IEnumerable<DezenArtikla> GetDezeniArtikla(long artikalID);

        bool IzbrisiDezenArtikla(long id);

        #endregion //DezeniArtikala

        #region Velicine

        long InsertOrUpdateVelicina(Velicina velicina);

        IEnumerable<Velicina> GetAllVelicine();

        IEnumerable<Velicina> GetVelicine(long artikalID);

        bool IzbrisiVelicinu(long id);

        #endregion //Velicine

        #region VelicineArtikla
        IEnumerable<VelicinaArtikla> GetFromVelicineArtikla(long artikalID);

        bool InsertOrUpdateVelicineArtikla(long artikalID, IEnumerable<long> velicineID);
        #endregion
    }
}
