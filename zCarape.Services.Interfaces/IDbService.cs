using System;
using System.Collections.Generic;
using System.Text;
using zCarape.Core.Models;
using zCarape.Core.Business;

namespace zCarape.Services.Interfaces
{
    public interface IDbService
    {
        bool TestConnection(out string poruka);
        bool NadogradiBazu(out string poruka);


        #region Masine
        IEnumerable<Masina> GetAllMasine();

        Masina GetByNazivMasina(string naziv);

        long InsertOrUpdateMasina(Masina masina);

        bool MasinaImaZavisneZapise(long id);

        bool DeleteMasina(long id);

        IEnumerable<MasinaURadu> GetAllMasineURadu(DateTime datum);

        IEnumerable<AngazovanaMasina> GetAngazovaneMasinePoRadnomNalogu(long radniNalogID);

        bool InsertOrUpdateAngazovaneMasine(long radniNalogID, IEnumerable<long> masineID);

        #endregion //Masine

        #region Artikli

        Artikal GetArtikal(long artikalID);

        IEnumerable<Artikal> GetAllArtikli();

        long InsertOrUpdateArtikal(Artikal artikal);

        bool IzbrisiArtikal(long artikalID);


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

        #region RadniNalozi

        long InsertOrUpdateRadniNalog(RadniNalog radniNalog);

        RadniNalog GetRadniNalog(long radniNalogID);

        bool DeleteRadniNalog(long radniNalogID);

        #endregion

        #region Predajnice

        /// <summary>
        /// Kreira ili azurira predajnicu. Ako zapis nije novi i ako je prosledjen parametar "kolona" onda azurira samo tu kolonu.
        /// </summary>
        /// <param name="predajnica"></param>
        /// <param name="kolona">Ako se ne unese onda ce sve kolone biti azuriranje</param>
        /// <returns></returns>
        long InsertOrUpdatePredajnica(Predajnica predajnica, string kolona = "");

        IEnumerable<Predajnica> GetAllPredajniceOnDate(DateTime dateTime);
        #endregion //Predajnica

        #region

        IEnumerable<Lice> GetAllAktivnaLica();

        #endregion
    }
}
