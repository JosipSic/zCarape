using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows;
using zCarape.Core;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;
using System.Linq;

namespace zCarape.Services
{
    public class DBService : IDbService
    {
        public bool TestConnection(out string poruka)
        {
            throw new NotImplementedException();
        }

        #region Masine
        public IEnumerable<MasinaURadu> GetAllMasineURadu()
        {
            List<MasinaURadu> masineURadu = new List<MasinaURadu>();

            // Za testiranje
            string slika1 =  Path.Combine(GlobalniKod.SlikeDir, "Bebino_Hulahop.jpg");
            string slika2 = Path.Combine(GlobalniKod.SlikeDir, "Muska_Termo_Carapa.jpg");
            string slika3 = Path.Combine(GlobalniKod.SlikeDir, "Muška sportska čarapa.jpg");
            string slika4 = Path.Combine(GlobalniKod.SlikeDir, "Zenska_Pamucna_Sokna.jpg");
            string slika5 = Path.Combine(GlobalniKod.SlikeDir, "Klasicna_Carapa_12.jpg");
            masineURadu.Add(new MasinaURadu()
            {
                MasinaID = 1,
                MasinaNaziv = "156 A",
                NaloziURadu = new List<NalogURadu>()
                {
                    new NalogURadu() { RadniNalogID=101, ArtikalID=1, ArtikalNaziv="Trend sokna muska", ArtikalVelicina="44/45", ArtikalDezen="Crna", Cilj=700, PutanjaFajla=@"2020\168\TrendSocks\44-45\"
                    ,Slika = slika1 },
                }
            });

            masineURadu.Add(new MasinaURadu()
            {
                MasinaID = 2,
                MasinaNaziv = "168 A",
                NaloziURadu = new List<NalogURadu>()
                {
                    new NalogURadu() { RadniNalogID=101, ArtikalID=1, ArtikalNaziv="Mediko", ArtikalVelicina="39/42", ArtikalDezen="Antracid", Cilj=1000, PutanjaFajla=@"2020\156A\Mediko\39-42\mana", Slika=slika2},
                    new NalogURadu() { RadniNalogID=102, ArtikalID=9, ArtikalNaziv="Sportska", ArtikalVelicina="42/43", ArtikalDezen="Plavo bela", Cilj=6500, PutanjaFajla=@"2020\156A\Sportska\42-43\plavobela", Slika=slika3},
                    new NalogURadu() { RadniNalogID=103, ArtikalID=3, ArtikalNaziv="Bebino", ArtikalVelicina="2,4", ArtikalDezen="Plavi automobili", Cilj=350, PutanjaFajla=@"2020\156A\Decije\automobilcici\2-4\maliplavi", Slika=slika4},

                }
            });


            masineURadu.Add(new MasinaURadu()
            {
                MasinaID = 3,
                MasinaNaziv = "XP6 156B",
                NaloziURadu = new List<NalogURadu>()
                {
                    new NalogURadu() { RadniNalogID=104, ArtikalID=26, ArtikalNaziv="Hulahop lovacki", ArtikalVelicina="univerzalna", ArtikalDezen="Rebrast", Cilj=100, PutanjaFajla=@"2020\156\Hulahop\Lovacki\Rebrast", Slika=slika5},
                }
            });

            return masineURadu;

        }

        public IEnumerable<Masina> GetAllMasine()
        {
            List<Masina> lista = new List<Masina>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM masine";

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {


                    lista.Add(new Masina()
                    {
                        ID = (long)dr["id"],
                        Naziv = (string)dr["naziv"],
                        Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                        Slika = (dr["slika"] == DBNull.Value) ? string.Empty : (string)dr["slika"],
                        Aktivan = (long)dr["aktivan"] ==1
                    }); ;
                    
                }
            }

            return lista;
        }

        public long InsertOrUpdateMasina(Masina masina)
        {
            bool noviZapis = masina.ID == 0;
            if (noviZapis)
            {
                return this.InsertMasina(masina);
            }
            else
            {
                return this.UpdateMasina(masina) ? masina.ID : 0;
            }
        }

        private bool UpdateMasina(Masina masina)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", masina.ID);
            cmd.Parameters.AddWithValue("@naziv", masina.Naziv);
            cmd.Parameters.AddWithValue("@opis", masina.Opis);
            cmd.Parameters.AddWithValue("@slika", masina.Slika);
            cmd.Parameters.AddWithValue("@aktivan", masina.Aktivan);

            cmd.CommandText = "UPDATE masina SET naziv=@naziv, opis=@opis, slika=@slika, aktivan=@aktivan WHERE id=@id";

            cmd.ExecuteNonQuery();

            return true;
        }

        private long InsertMasina(Masina masina)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@naziv", masina.Naziv);
            cmd.Parameters.AddWithValue("@opis", masina.Opis);
            cmd.Parameters.AddWithValue("@slika", masina.Slika);
            cmd.Parameters.AddWithValue("@aktivan", masina.Aktivan);

            cmd.CommandText = "INSERT INTO masine (naziv,opis,slika,aktivan) VALUES (@naziv,@opis,@slika,@aktivan)";

            if (cmd.ExecuteNonQuery() == 1)
                return con.LastInsertRowId;
            else
                return masina.ID;
        }


        public bool DeleteMasina(long id)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.CommandText = "DELETE FROM masina WHERE id=@id";
            return cmd.ExecuteNonQuery() == 1;
        }

        public Masina GetByNazivMasina(string naziv)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM Masine WHERE naziv=@naziv";
            cmd.Parameters.AddWithValue("@naziv", naziv);

            using SQLiteDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

            if (dr.HasRows)
            {
                dr.Read();


                return new Masina()
                {
                    ID = (long)dr["id"],
                    Naziv = (string)dr["naziv"],
                    Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                    Slika = (dr["slika"] == DBNull.Value) ? string.Empty : (string)dr["slika"],
                    Aktivan = (long)dr["aktivan"]==1,
                    VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                };
            }
            else
            {
                return null;
            }
        }

        public bool MasinaImaZavisneZapise(long id)
        {
            // TODO
            return false;
        }

        #endregion // Masine

        #region Artikli
        public Artikal GetArtikal(long artikalID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM Artikli WHERE ID=@id";
            cmd.Parameters.AddWithValue("@id", artikalID);

            using SQLiteDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

            if (dr.HasRows)
            {
                dr.Read();


                return new Artikal()
                {
                    ID = (long)dr["id"],
                    Sifra = (string)dr["sifra"],
                    Naziv = (string)dr["naziv"],
                    Jm = (string)dr["jm"],
                    Slika = (dr["slika"] == DBNull.Value) ? string.Empty : (string)dr["slika"],
                    BarKod = (dr["barkod"] == DBNull.Value) ? string.Empty : (string)dr["barkod"],
                    VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                };
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Artikal> GetAllArtikli()
        {
            List<Artikal> lista = new List<Artikal>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM artikli";

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {


                    lista.Add(new Artikal()
                    {
                        ID = (long)dr["id"],
                        Sifra = (string)dr["sifra"],
                        Naziv = (string)dr["naziv"],
                        Jm = (string)dr["jm"],
                        Slika = (dr["slika"] == DBNull.Value) ? string.Empty : (string)dr["slika"],
                        BarKod = (dr["barkod"] == DBNull.Value) ? string.Empty : (string)dr["barkod"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    }); ;

                }
            }

            return lista;
        }


        public long InsertOrUpdateArtikal(Artikal artikal)
        {
            bool noviZapis = artikal.ID == 0;
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@sifra", artikal.Sifra);
            cmd.Parameters.AddWithValue("@naziv", artikal.Naziv);
            cmd.Parameters.AddWithValue("@jm", artikal.Jm);
            cmd.Parameters.AddWithValue("@slika", artikal.Slika);
            cmd.Parameters.AddWithValue("@barkod", artikal.BarKod);

            if (noviZapis)
            {
                cmd.CommandText = "INSERT INTO artikli (sifra,naziv,jm,slika,barkod) VALUES (@sifra,@naziv,@jm,@slika,@barkod)";
                long odgovor = 0;
                try
                {
                    odgovor = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Šifra artikla \"{artikal.Sifra}\" već postoji u bazi."),
                            "Nisu dozvoljene duple šifre.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

                if (odgovor == 1)
                    return con.LastInsertRowId;
            }
            else
            {
                cmd.Parameters.AddWithValue("@id", artikal.ID);
                cmd.CommandText = "UPDATE artikli SET sifra=@sifra, naziv=@naziv, jm=@jm, slika=@slika, barkod=@barkod WHERE id=@id";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Šifra artikla \"{artikal.Sifra}\" već postoji u bazi."),
                            "Nisu dozvoljene duple šifre.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

            }
            return artikal.ID;

        }


        #endregion

        #region DezeniArtikala

        public long InsertOrUpdateDezenArtikla(DezenArtikla dezenArtikla)
        {
            bool noviZapis = dezenArtikla.ID == 0;
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@artikalid", dezenArtikla.ArtikalID);
            cmd.Parameters.AddWithValue("@naziv", dezenArtikla.Naziv);
            cmd.Parameters.AddWithValue("@opis", dezenArtikla.Opis);
            cmd.Parameters.AddWithValue("@slika1", dezenArtikla.Slika1);
            cmd.Parameters.AddWithValue("@slika2", dezenArtikla.Slika2);
            cmd.Parameters.AddWithValue("@slika3", dezenArtikla.Slika3);
            cmd.Parameters.AddWithValue("@aktivan", dezenArtikla.Aktivan);

            if (noviZapis)
            {
                cmd.CommandText = "INSERT INTO dezeniartikla (artikalid,naziv,opis,slika1,slika2,slika3,aktivan) VALUES (@artikalid,@naziv,@opis,@slika1,@slika2,@slika3,@aktivan)";
                long odgovor = 0;
                try
                {
                    odgovor = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Naziv dezen \"{dezenArtikla.Naziv}\" za izabrani artikal već postoji u bazi."),
                            "Nisu dozvoljene dupli nazivi dezena za isti artikal.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

                if (odgovor == 1)
                    return con.LastInsertRowId;
            }
            else
            {
                cmd.Parameters.AddWithValue("@id", dezenArtikla.ID);
                cmd.CommandText = "UPDATE dezeniartikla SET artikalid=@artikalid, naziv=@naziv,  opis=@opis, slika1=@slika1 " +
                    ",slika2=@slika2, slika3=@slika3, aktivan=@aktivan WHERE id=@id";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Naziv dezen \"{dezenArtikla.Naziv}\" za izabrani artikal već postoji u bazi."),
                            "Nisu dozvoljene dupli nazivi dezena za isti artikal.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

            }
            return dezenArtikla.ID;
        }

        public IEnumerable<DezenArtikla> GetDezeniArtikla(long artikalID)
        {
            List<DezenArtikla> lista = new List<DezenArtikla>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM DezeniArtikla WHERE ArtikalID=@artikalID";
            cmd.Parameters.AddWithValue("@artikalID", artikalID);

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(
                        new DezenArtikla() 
                        { 
                            ID = (long)dr["id"], 
                            ArtikalID = (long)dr["artikalid"], 
                            Naziv = (string)dr["naziv"],
                            Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                            Slika1 = (dr["slika1"] == DBNull.Value) ? string.Empty : (string)dr["slika1"],
                            Slika2 = (dr["slika2"] == DBNull.Value) ? string.Empty : (string)dr["slika2"],
                            Slika3 = (dr["slika3"] == DBNull.Value) ? string.Empty : (string)dr["slika3"],
                            VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                        }
                    );
                }
            }

            return lista;
        }

        public DezenArtikla GetDezenArtikla(long dezenArtiklaID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM DezeniArtikla WHERE ID=@id";
            cmd.Parameters.AddWithValue("@id", dezenArtiklaID);

            using SQLiteDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

            if (dr.HasRows)
            {
                dr.Read();

                return new DezenArtikla()
                {
                    ID = (long)dr["id"],
                    ArtikalID = (long)dr["artikalid"],
                    Naziv = (string)dr["naziv"],
                    Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                    Slika1 = (dr["slika1"] == DBNull.Value) ? string.Empty : (string)dr["slika1"],
                    Slika2 = (dr["slika2"] == DBNull.Value) ? string.Empty : (string)dr["slika2"],
                    Slika3 = (dr["slika3"] == DBNull.Value) ? string.Empty : (string)dr["slika3"],
                    VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                };
            }
            else
            {
                return null;
            }
        }

        public bool IzbrisiDezenArtikla(long id)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.CommandText = "DELETE FROM DezeniArtikla WHERE id=@id";
            return cmd.ExecuteNonQuery() == 1;
        }

        #endregion //DezeniArtikala

        #region Velicine

        public long InsertOrUpdateVelicina(Velicina velicina)
        {
            if (string.IsNullOrWhiteSpace(velicina.Oznaka))
            {
                return 0;
            }

            bool noviZapis = velicina.ID == 0;
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@oznaka", velicina.Oznaka);

            if (noviZapis)
            {
                cmd.CommandText = "INSERT INTO velicine (oznaka) VALUES (@oznaka)";

                long odgovor = 0;
                try
                {
                    odgovor = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode==19)
                    {
                        MessageBox.Show(String.Format($"Veličina sa oznakom \"{velicina.Oznaka}\" već postoji u bazi."),
                            "Nisu dozvoljeni dupli unosi",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

                if (odgovor == 1)
                    return con.LastInsertRowId;
            }
            else
            {
                cmd.Parameters.AddWithValue("@id", velicina.ID);
                cmd.CommandText = "UPDATE velicine SET oznaka=@oznaka WHERE id=@id";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Veličina sa oznakom \"{velicina.Oznaka}\" već postoji u bazi."),
                            "Nisu dozvoljeni dupli unosi", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }
            }
            return velicina.ID;
        }

        public IEnumerable<Velicina> GetAllVelicine()
        {
            List<Velicina> lista = new List<Velicina>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM velicine ORDER BY oznaka";

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {


                    lista.Add(new Velicina()
                    {
                        ID = (long)dr["id"],
                        Oznaka = (string)dr["oznaka"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    }); ;

                }
            }

            return lista;
        }

        public IEnumerable<Velicina> GetVelicine(long artikalID)
        {
            List<Velicina> lista = new List<Velicina>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT Velicine.* FROM Velicine INNER JOIN VelicineArtikla ON Velicine.ID=VelicineArtikla.VelicinaID WHERE VelicineArtikla.ArtikalID=@artikalID ORDER BY Velicine.Oznaka";
            cmd.Parameters.AddWithValue("@artikalID", artikalID);

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(new Velicina()
                    {
                        ID = (long)dr["id"],
                        Oznaka = (string)dr["oznaka"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    });
                }
            }

            return lista;
        }


        public bool IzbrisiVelicinu(long id)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.CommandText = "DELETE FROM velicine WHERE id=@id";
            return cmd.ExecuteNonQuery() == 1;
        }


        #endregion //Velicine

        #region VelicineArtikla
        public IEnumerable<VelicinaArtikla> GetFromVelicineArtikla(long artikalID)
        {
            List<VelicinaArtikla> lista = new List<VelicinaArtikla>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM VelicineArtikla WHERE VelicineArtikla.ArtikalID=@artikalID";
            cmd.Parameters.AddWithValue("@artikalID", artikalID);

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(new VelicinaArtikla()
                    {
                        ID = (long)dr["id"],
                        ArtikalID = (long)dr["artikalid"],
                        VelicinaID = (long)dr["velicinaid"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    }); ;
                }
            }

            return lista;
        }


        public bool InsertOrUpdateVelicineArtikla(long artikalID, IEnumerable<long> noveVelicineID)
        {
            if (artikalID==0)
            {
                MessageBox.Show("artikalID ne moze biti 0. Poruka iz procedure InsertOrUpdateVelicineArtikla", "Nije moguce snimiti velicine");
                return false;
            }

            //1-Iz baze ucitavam sve velicine za trazeni artikal
            IEnumerable<VelicinaArtikla> izBaze = GetFromVelicineArtikla(artikalID);

            //2-Brisem uklonjene velicine (iz kolekcije izBaze sve zapise koji nemaju VelicinaID da se nalazi u kolekciji velicineID)
            var uklonjeneVelicine = from va in izBaze
                                    where va.ArtikalID == artikalID && !noveVelicineID.Contains(va.VelicinaID)
                                    select va.ID;

            foreach (long uklonjenID in uklonjeneVelicine)
            {
                using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                con.Open();
                using var cmd = new SQLiteCommand(con);
                cmd.Parameters.AddWithValue("@id", uklonjenID);
                cmd.CommandText = "DELETE FROM velicineartikla WHERE id=@id";
                cmd.ExecuteNonQuery();
            }

            //3-Dodajem nove velicine
            var dodateVelicine = from n in noveVelicineID
                                 where !izBaze.Any(b => b.VelicinaID == n)
                                 select n;

            foreach (long dodataVelicinaID in dodateVelicine)
            {
                insertVelicinaArtikla(artikalID, dodataVelicinaID);
            }

            return true;
        }

        private long insertVelicinaArtikla(long artikalID, long velicinaID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@artikalid", artikalID);
            cmd.Parameters.AddWithValue("@velicinaid", velicinaID);

            cmd.CommandText = "INSERT INTO velicineartikla (artikalid,velicinaid) VALUES (@artikalid,@velicinaid)";

            if (cmd.ExecuteNonQuery() == 1)
                return con.LastInsertRowId;
            else
                return 0;

        }

        #endregion
    }
}
